using AutoMapper;
using ChatAppAPI.Data;
using ChatAppAPI.Models;
using ChatAppAPI.Models.AuthDTO;
using ChatAppAPI.Models.AuthModels;
using ChatAppAPI.Models.UserDTO;
using ChatAppAPI.Repository;
using ChatAppAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChatAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiExplorerSettings(GroupName = "1-Auth")]
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
        public async Task<IActionResult> Login1([FromBody] Models.AuthDTO.LoginRequest request)
        {
            var user = await _repository.GetAsync(x => x.Email == request.Email);

            if (user == null) return Unauthorized("User not found");
            if (user.Password != request.Password) return Unauthorized("Invalid password");

            var Response = _jwt.Authenticate(request);

            return Ok(Response.Result);
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] Models.AuthDTO.RegisterRequest request)
        {
            if (request == null) return BadRequest("null request");
            Console.WriteLine(request.Email);
            var validUser = await _repository.GetAsync(x => x.Email == request.Email) == null ? true : false;
            if(!validUser) return BadRequest("User with that email already exits!");
            var user = _mapper.Map<User>(request);
            user.CreatedAt = DateTime.UtcNow;
            var result = await _repository.AddAsync(user);
            return Ok(result);
        }
    }
}