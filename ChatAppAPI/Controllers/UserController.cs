using AutoMapper;
using ChatAppAPI.Data;
using ChatAppAPI.Models;
using ChatAppAPI.Models.UserDTO;
using ChatAppAPI.Repository;
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
        private readonly IUserReporitory _repository;
        private readonly IMapper _mapper;

        public UserController(ILogger<UserController> logger, DataContext data, IUserReporitory repo, IMapper mapper)
        {
            _logger = logger;
            _data = data;
            _repository = repo;
            _mapper = mapper;
        }

        [HttpGet("GetAll", Name = "GetAllUsers")]
        public async Task<IActionResult> GetAllUsers() {
            var AllUsers = await _repository.GetAllAsync();
            if (AllUsers == null)
            {
                return NotFound("Users doesn't found");
            }
            var Dtos = _mapper.Map<List<UserDTO>>(AllUsers);
            return Ok(Dtos);
        }


        [HttpGet("GetById/{id:int}", Name = "GetUserById")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var found = await _repository.GetAsync(x => x.Id == id);
            if (found == null)
                return NotFound("Not found user");
            var Dto = _mapper.Map<UserDTO>(found);
            return Ok(Dto);
        }

        [HttpPost("Create", Name = "CreateUser")]
        public async Task<IActionResult> CreateUser([FromBody] UserCreateDTO create)
        {
            if (create == null)
                return BadRequest();
            var ValidMail = await _repository.GetAsync(x => x.Email == create.Email) == null ? true : false;
            if (!ValidMail) {
                return BadRequest("Account with same email already exists");
            }
            var account = _mapper.Map<User>(create);
            account.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(account);
            return Ok(account);
        }

        [HttpPost("Login",Name = "LoginUser")]
        public async Task<IActionResult> Login([FromBody] UserLoginDTO login)
        {
            var user = await _repository.GetAsync(x => x.Email == login.Email);
            if (user == null)
            {
                return Unauthorized("Account by email doesn't found");
            }
            if (user.Password != login.Password)
            {
                return Unauthorized("Password is not correct");
            }
            var dto = _mapper.Map<UserDTO>(user);
            return Ok(dto);
        }

        [HttpPatch("Update", Name = "UpdateUser")]
        public async Task<IActionResult> Update([FromBody] UserUpdateDTO user)
        {
            if(user == null) return BadRequest();
            var User = await _repository.GetAsync(x => x.Id == user.Id);
            if (User == null) {
                return Unauthorized("Cant find user");
            }
            var dto = _mapper.Map<User>(user);
            var result = await _repository.UpdateAsync(dto);
            return Ok(result);
        }

        [HttpDelete("Delete", Name = "DeleteUser")]
        public async Task<IActionResult> Delete([FromBody] UserDeleteDTO user)
        {
            if (user == null) return BadRequest();
            var User = await _repository.GetAsync(x => x.Id == user.Id);
            if (User == null) return NotFound("Account with that Id don't exists");
            if (user.Password != User.Password) return Unauthorized("Wrong password");
            var result = await _repository.RemoveAsync(User);
            return Ok(result);
        }
    }
}
