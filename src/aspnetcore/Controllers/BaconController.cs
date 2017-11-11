using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace NotBacon.Controllers
{
    [Route("api/[controller]")]
    public class BaconController : Controller
    {
        private readonly IConfiguration config;
        private readonly HttpClient httpClient;

        public BaconController(IConfiguration config, HttpClient httpClient)
        {
            this.config = config;
            this.httpClient = httpClient;
        }
        
        [HttpGet]
        public async Task<BaconResult> Get(string url)
        {
            var hasBacon = await ContainsBacon(url);
            var hasKevinBacon = await ContainsKevinBacon(url);

            return new BaconResult
            {
                HasBacon = hasBacon,
                HasKevinBacon = hasKevinBacon
            };
        }

        private async Task<bool> ContainsBacon(string url)
        {
            var content = new StringContent(JsonConvert.SerializeObject(new { Url = url }));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.Add("Prediction-Key", config["CUSTOM_VISION_API_KEY"]);

            var response = await httpClient.PostAsync(config["CUSTOM_VISION_API_URL"], content);
            var result = JsonConvert.DeserializeObject<CustomVisionResult>(await response.Content.ReadAsStringAsync());

            var baconPrediction = result.Predictions.FirstOrDefault(p => p.Tag == "bacon");
            var baconProbability = baconPrediction?.Probability ?? 0;
            return baconProbability > 0.7m;
        }

        private async Task<bool> ContainsKevinBacon(string url)
        {
            var content = new StringContent(JsonConvert.SerializeObject(new { url }));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.Add("Ocp-Apim-Subscription-Key", config["COMP_VISION_API_KEY"]);

            var response = await httpClient.PostAsync(config["COMP_VISION_API_URL"], content);
            var resultJson = await response.Content.ReadAsStringAsync();

            return Regex.IsMatch(resultJson, @"\bkevin bacon\b", RegexOptions.IgnoreCase);
        }

        private class Prediction
        {
            public string Tag { get; set; }
            public decimal Probability { get; set; }
        }

        private class CustomVisionResult
        {
            public Prediction[] Predictions { get; set; }
        }

        public class BaconResult
        {
            public bool HasBacon { get; set; }
            public bool HasKevinBacon { get; set; }
        }
    }
}

