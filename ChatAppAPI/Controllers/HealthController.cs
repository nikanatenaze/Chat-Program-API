using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppAPI.Controllers
{
    [ApiExplorerSettings(GroupName = "6-Health")]
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
