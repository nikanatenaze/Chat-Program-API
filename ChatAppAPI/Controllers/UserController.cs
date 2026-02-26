using AutoMapper;
using ChatAppAPI.Models;
using ChatAppAPI.Models.UserDTO;
using ChatAppAPI.Repository;
using ChatAppAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace ChatAppAPI.Controllers
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "2-Users")]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;
        private readonly CloudinaryService _cloudinary;

        public UserController(
            ILogger<UserController> logger, 
            IUserRepository repo,
            IMapper mapper,
            CloudinaryService cloudinary
            )
        {
            _logger = logger;
            _repository = repo;
            _mapper = mapper;
            _cloudinary = cloudinary;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAll", Name = "GetAllUsers")]
        public async Task<IActionResult> GetAllUsers() {
            var AllUsers = await _repository.GetAllAsync();
            var Dtos = _mapper.Map<List<UserDTO>>(AllUsers);
            return Ok(Dtos);
        }

        [HttpGet("GetCurrentUserInfo", Name = "GetCurrentUserInfo")]
        public IActionResult GetCurrentUserInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var name = User.FindFirstValue(ClaimTypes.Name);
            var role = User.FindFirstValue(ClaimTypes.Role);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid token: missing user ID");

            return Ok(new { Id = userId, Name = name, Email = email, Role = role});
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

        [HttpGet("Search/{name}")]
        public async Task<IActionResult> SearchUserByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest("Search term required");

            var users = await _repository.GetAllAsync(
                x => 
                x.Name != null &&
                EF.Functions.Like(x.Name.ToLower(), $"%{name.ToLower()}%"));

            var usersDto = _mapper.Map<List<UserDTO>>(users);
            return Ok(usersDto);
        }

        [HttpPost("UploadProfileImage")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadProfileImage([FromForm] UserProfileImageDTO dto)
        {
            if (dto?.Image == null || dto.Image.Length == 0)
                return BadRequest("No image uploaded");

            var userId = GetCurrentUserId();
            var user = await _repository.GetAsync(x => x.Id == userId);

            if (user == null)
                return NotFound("User not found");

            var imageUrl = await _cloudinary.UploadImageAsync(dto.Image);

            user.ProfileImageUrl = imageUrl;

            await _repository.UpdateAsync(user);

            return Ok(new { ImageUrl = imageUrl });
        }

        [HttpPatch("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] UserPasswordDTO dto)
        {
            if (dto == null) return BadRequest();

            var userId = GetCurrentUserId();
            var user = await _repository.GetAsync(x => x.Id == userId);

            if (user == null)
                return NotFound("User not found");

            if (user.Password != dto.CurrentPassword)
                return Unauthorized("Wrong current password");

            user.Password = dto.NewPassword;

            await _repository.UpdateAsync(user);

            return Ok("Password updated");
        }

        [HttpPatch("Update", Name = "UpdateUser")]
        public async Task<IActionResult> Update([FromBody] UserUpdateDTO dto)
        {
            if (dto == null) return BadRequest();

            var userId = GetCurrentUserId();
            var user = await _repository.GetAsync(x => x.Id == userId);

            if (user == null)
                return NotFound("User not found");

            if (!string.IsNullOrEmpty(dto.Name))
                user.Name = dto.Name;

            if (!string.IsNullOrEmpty(dto.Email))
            {
                if (!new EmailAddressAttribute().IsValid(dto.Email))
                    return BadRequest("Invalid email");
                user.Email = dto.Email;
            }

            var result = await _repository.UpdateAsync(user);
            var resultDto = _mapper.Map<UserDTO>(result);

            return Ok(resultDto);
        }

        [HttpDelete("Delete", Name = "DeleteUser")]
        public async Task<IActionResult> Delete([FromBody] UserDeleteDTO user)
        {
            if (user == null) return BadRequest();
            
            var userId = GetCurrentUserId();
            var User = await _repository.GetAsync(x => x.Id == userId);

            if (User == null) return NotFound("Account with that Id don't exists");
            if (user.Password != User.Password) return Unauthorized("Wrong password");

            await _repository.RemoveAsync(User);

            return NoContent();
        }

        // helper methods

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !int.TryParse(claim.Value, out int userId))
                throw new UnauthorizedAccessException("Invalid User Id");

            return userId;
        }
    }
}
