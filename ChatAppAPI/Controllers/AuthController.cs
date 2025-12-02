using ChatAppAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtService _jwt;

        public AuthController(JwtService jwt)
        {
            _jwt = jwt;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest model)
        {
            // TODO: Validate user existence from DB
            if (model.Username != "test" || model.Password != "1234")
                return Unauthorized("Invalid credentials");

            var token = _jwt.GenerateToken("1", model.Username);

            return Ok(new { token });
        }
    }

    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
