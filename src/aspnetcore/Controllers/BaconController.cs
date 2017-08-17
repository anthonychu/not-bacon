using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
        public async Task<object> Get(string url)
        {
            var content = new StringContent(JsonConvert.SerializeObject(new { Url = url }));
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            content.Headers.Add("Prediction-Key", config["CUSTOM_VISION_API_KEY"]);

            var response = await httpClient.PostAsync(config["CUSTOM_VISION_API_URL"], content);
            var result = JsonConvert.DeserializeObject<CustomVisionResult>(await response.Content.ReadAsStringAsync());

            var baconPrediction = result.Predictions.FirstOrDefault(p => p.Tag == "bacon");
            var baconProbability = baconPrediction?.Probability ?? 0;

            return new BaconResult
            {
                HasBacon = baconProbability > 0.7m
            };
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
        }
    }
}

