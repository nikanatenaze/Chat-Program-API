using AutoMapper;
using ChatAppAPI.Data;
using ChatAppAPI.Models;
using ChatAppAPI.Models.ChatDTO;
using ChatAppAPI.Models.UserDTO;
using ChatAppAPI.Repository;
using ChatAppAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ChatAppAPI.Controllers
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "3-Chats")]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IChatRepository _repository;
        private readonly IUserRepository _userReporitory;
        private readonly IMessageRepository _messageRepository;
        private readonly IChatUserRepository _chatUserRepository;
        private readonly CloudinaryService _cloudinary;
        private readonly IMapper _mapper;

        public ChatController(ILogger<ChatController> logger, 
            DataContext data,
            IChatRepository repo,
            IUserRepository userReporitory, 
            IMessageRepository messageRepository,
            IMapper mapper,
            IChatUserRepository chatUserRepository,
            CloudinaryService cloudinary
            )
        {
            _logger = logger;
            _repository = repo;
            _mapper = mapper;
            _userReporitory = userReporitory;
            _messageRepository = messageRepository;
            _chatUserRepository = chatUserRepository;
            _cloudinary = cloudinary;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAll", Name = "GetAllChats")]
        public async Task<IActionResult> GetAllChats()
        {
            var AllChats = await _repository.GetAllAsync();
            var Dtos = _mapper.Map<List<ChatDTO>>(AllChats);

            return Ok(Dtos);
        }

        [HttpGet("GetById/{id:int}", Name = "GetChatById")]
        public async Task<IActionResult> GetChatById(int id)
        {
            var userId = GetCurrentUserId();
            var adminRole = User.IsInRole("Admin");
            var isMemb = await _chatUserRepository.IsUserInChat(userId, id);
            if (!isMemb && !adminRole) return Forbid("Your not member of chat!");

            var found = await _repository.GetAsync(x => x.Id == id);
            if (found == null)
                return NotFound("Not found Chat");

            var Dto = _mapper.Map<ChatDTO>(found);

            return Ok(Dto);
        }


        [HttpGet("GetMessages/{id:int}", Name = "GetChatMessages")]
        public async Task<IActionResult> GetChatMessages(int id)
        {
            if (id == 0) return BadRequest("Id can't be null");

            var userId = GetCurrentUserId();
            var isMemb = await _chatUserRepository.IsUserInChat(userId, id);
            if (!isMemb) return Forbid("Your not member of chat!");

            var chat = await _repository.GetAsync(x => x.Id == id);
            if (chat == null) return NotFound("Can't find chat");

            var messages = await _messageRepository.GetAllAsync(x => x.ChatId == id);

            return Ok(messages);
        }

        [HttpPost("Create", Name = "CreateChat")]
        public async Task<IActionResult> CreateChat([FromBody] ChatCreateDTO create)
        {
            if (create == null)
                return BadRequest();

            var userId = GetCurrentUserId();
            var user = await _userReporitory.GetAsync(x => x.Id == userId);
            if (user == null) return BadRequest("Cant find user with that Id");

            var chat = _mapper.Map<Chat>(create);
            chat.CreatedByUserId = userId;
            chat.HasPassword = !string.IsNullOrEmpty(create.Password);
            chat.CreatedAt = DateTime.UtcNow;

            var result = await _repository.AddAsync(chat);
            var resultDto = _mapper.Map<ChatDTO>(result);

            await _chatUserRepository.AddAsync(new ChatUser
            {
                ChatId = result.Id,
                UserId = userId
            });

            return Ok(resultDto);
        }

        [HttpPost("UploadChatImage")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadChatImage([FromForm] ChatImageDTO dto)
        {
            if (dto?.Image == null || dto.Image.Length == 0)
                return BadRequest("No image uploaded");

            var chat = await _repository.GetAsync(x => x.Id == dto.ChatId);
            if (chat == null) return NotFound("Chat not found");

            // Optional: authorization check
            var currentUserId = GetCurrentUserId();
            var isCreator = await _repository.IsUserCreator(currentUserId, dto.ChatId);
            if (!isCreator && !User.IsInRole("Admin"))
                return Forbid("You are not allowed to update this chat image");

            var imageUrl = await _cloudinary.UploadImageAsync(dto.Image);

            chat.ChatImageUrl = imageUrl;

            await _repository.UpdateAsync(chat);

            return Ok(new { ImageUrl = imageUrl });
        }

        [HttpPost("ChatVerification", Name = "LoginInChat")]
        public async Task<IActionResult> LoginInChat([FromBody] ChatLoginDTO dto)
        {
            var chat = await _repository.GetAsync(x => x.Id == dto.ChatId);
            if(chat == null) return Unauthorized("cant find chat");

            if(!chat.HasPassword) return BadRequest("chat has no password");

            if(chat.Password != dto.Password) return Unauthorized("wrong password");

            var userId = GetCurrentUserId();
            var isMember = await _chatUserRepository.IsUserInChat(userId, chat.Id);

            if (!isMember)
            {
                await _chatUserRepository.AddAsync(new ChatUser() { 
                    ChatId = chat.Id,
                    UserId = userId,
                    JoinedAt = DateTime.UtcNow,
                });
            }
            var cDto = _mapper.Map<ChatDTO>(chat);

            return Ok(cDto);
        }

        [HttpPatch("Update", Name = "UpdateChat")]
        public async Task<IActionResult> Update([FromBody] ChatUpdateDTO dto)
        {
            if (dto == null) return BadRequest();

            var userId = GetCurrentUserId();
            var isCreator = await _repository.IsUserCreator(userId, dto.Id);
            if (!isCreator) return Forbid("You are not chat owner");

            var aChat = await _repository.GetAsync(x => x.Id == dto.Id);
            if (aChat == null) return NotFound();

            _mapper.Map(dto, aChat);
            var result = await _repository.UpdateAsync(aChat);
            var resultDto = _mapper.Map<ChatDTO>(result);

            return Ok(resultDto);
        }

        [HttpDelete("DeleteById/{Id:int}", Name = "DeleteChat")]
        public async Task<IActionResult> Delete(int Id)
        {

            if(Id == 0) return BadRequest();

            var userId = GetCurrentUserId();

            var adminRole = User.IsInRole("Admin");
            var isCreator = await _repository.IsUserCreator(userId, Id);
            if (!isCreator && !adminRole) return Forbid("You can delete only own chats!");

            var chat = await _repository.GetAsync(x => x.Id == Id);
            if (chat == null) return NotFound("Chat with that Id don't exists");

            var result = await _repository.RemoveAsync(chat);
            return Ok(result);
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

