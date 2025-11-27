using ChatAppAPI.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DataContext _data;
        private readonly ILogger<UserController> _logger;

        public UserController(ILogger<UserController> logger, DataContext data)
        {
            _logger = logger;
            _data = data;
        }

        [HttpGet("GetAllUsers", Name = "GetAllUsers")]
        public IActionResult GetAllUsers() {
            return null;
        }


        [HttpGet("GetUserById/{id:int}", Name = "GetUserById")]
        public IActionResult GetUserById(int id)
        {
            var found = _data.Users.FirstOrDefault(x => x.Id == id);
            return found == null ? NotFound("Not found user by id") : Ok(found);
        }
    }
}
