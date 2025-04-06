// Controller for handling API requests
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;

namespace WApp.Controllers
{
    [Route("api")]
    public class PingController : Controller
    {
        private readonly IHttpClientFactory _clientFactory;

        public PingController(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        [HttpPost("ping")]
        public IActionResult ReceivePing()
        {
            return Json(new { source = ".NET App (W)", message = "Received ping!" });
        }

        [HttpPost("ping-l")]
        public async Task<IActionResult> PingL()
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var response = await client.PostAsync("http://192.168.1.111/api/ping", null);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                
                return Json(new { status = "success", message = JsonConvert.DeserializeObject(content) });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [HttpPost("ping-k")]
        public async Task<IActionResult> PingK()
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var response = await client.PostAsync("http://k8s-app-url/api/ping", null);

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                
                return Json(new { status = "success", message = JsonConvert.DeserializeObject(content) });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }

        [HttpGet("cat-fact")]
        public async Task<IActionResult> GetCatFact()
        {
            try
            {
                var client = _clientFactory.CreateClient();
                var response = await client.GetAsync("https://catfact.ninja/fact");

                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                
                return Json(new { status = "success", data = JsonConvert.DeserializeObject(content) });
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = ex.Message });
            }
        }
    }
}