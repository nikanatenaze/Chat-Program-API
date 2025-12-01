using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppAPI.Controllers
{
    [ApiExplorerSettings(GroupName = "5-HealthController")]
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet, HttpPost, HttpPut, HttpDelete, HttpHead]
        public IActionResult Get()
        {
            return Ok("Api is running");
        }
    }
}
