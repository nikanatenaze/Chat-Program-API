using AutoMapper;
using ChatAppAPI.Data;
using ChatAppAPI.Models;
using ChatAppAPI.Models.AuthDTO;
using ChatAppAPI.Models.AuthModels;
using ChatAppAPI.Models.UserDTO;
using ChatAppAPI.Repository;
using ChatAppAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserReporitory _repository;
        private readonly JwtService _jwt;

        public AuthController(IMapper mapper,IUserReporitory reporitory, JwtService jwt)
        {
            _mapper=mapper;
            _repository = reporitory;
            _jwt = jwt;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDTO>> Login(LoginRequestDTO request)
        {
            var result = await _jwt.Authenticate(request);
            if (result == null) { return Unauthorized();  }
            return Ok(result);
        }

/*        [AllowAnonymous]
        [HttpPost("login1")]
        public async Task<IActionResult> Login1([FromBody] LoginRequestDTO request)
        {
            var user = await _repository.GetAsync(x => x.Email == request.Email);

            if (user == null) return Unauthorized("User not found");
            if (user.Password != request.Password) return Unauthorized("Invalid password");
            
            var token = _jwt.GenerateToken(user.Id.ToString(), user.Email);
            var userDTO = _mapper.Map<UserDTO>(user);

            return Ok(new LoginResponseDTO { Token = token, User = userDTO});
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request == null) return BadRequest("null request");
            var user = _mapper.Map<User>(request);
            user.CreatedAt = DateTime.UtcNow;
            var result = await _repository.AddAsync(user);
            return Ok(result);
        }*/
    }
}