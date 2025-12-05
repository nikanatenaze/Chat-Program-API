using AutoMapper;
using ChatAppAPI.Data;
using ChatAppAPI.Models;
using ChatAppAPI.Models.ChatDTO;
using ChatAppAPI.Models.ChatUserDTO;
using ChatAppAPI.Models.UserDTO;
using ChatAppAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppAPI.Controllers
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "3-ChatUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatUserController : ControllerBase
    {
        private readonly ILogger<ChatUserController> _logger;
        private readonly IChatUserRepository _repository;
        private readonly IUserReporitory _userRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IMapper _mapper;

        public ChatUserController(ILogger<ChatUserController> logger, IChatUserRepository repo, IUserReporitory userRepository, IChatRepository chatRepository, IMapper mapper)
        {
            _logger = logger;
            _repository = repo;
            _mapper = mapper;
            _userRepository = userRepository;
            _chatRepository = chatRepository;
        }

        [HttpGet("GetAll", Name = "GetAllChatUsers")]
        public async Task<IActionResult> GetAllChatUsers()
        {
            var result = await _repository.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("GetUsersInChat/{Id:int}", Name = "GetUsersInChat")]
        public async Task<IActionResult> GetUsersInChat(int Id)
        {
            var ids = await _repository.GetAllAsync(x => x.ChatId == Id);
            var userIds = ids.Select(x => x.UserId).ToList();
            var users = await _userRepository.GetAllAsync(x => userIds.Contains(x.Id));
            var result = _mapper.Map<List<UserDTO>>(users);
            return Ok(result);
        }

        [HttpGet("GetChatsOfUser/{Id:int}", Name = "GetChatsOfUser")]
        public async Task<IActionResult> GetChatsOfUser(int Id)
        {
            var ids = await _repository.GetAllAsync(x => x.UserId == Id);
            var chatIds = ids.Select(x => x.ChatId).ToList();
            var chats = await _chatRepository.GetAllAsync(x => chatIds.Contains(x.Id));
            var result = _mapper.Map<List<ChatDTO>>(chats);
            return Ok(result);
        }

        [HttpGet("{chatId:int}/isMember/{userId:int}")]
        public async Task<IActionResult> IsMember(int chatId, int userId)
        {
            var chatUser = await _repository.GetAsync(x => x.ChatId == chatId && x.UserId == userId);
            return Ok(chatUser != null);
        }

        [HttpPost("AddChatUser", Name = "AddChatUser")]
        public async Task<IActionResult> AddChatUser([FromBody] ChatUserDTO item)
        {
            var user = await _userRepository.GetAsync(x => x.Id == item.UserId);
            if (user == null) return NotFound("Can't find user");
            var chat = await _chatRepository.GetAsync(x => x.Id == item.ChatId);
            if (chat == null) return NotFound("Can't find chat");
            var exits = await _repository.GetAsync(x => x.UserId == item.UserId && x.ChatId == item.ChatId);
            if (exits != null) return BadRequest("ChatUser already exits");
            var resp = await _repository.AddAsync(new ChatUser { ChatId = item.ChatId, UserId = item.UserId, JoinedAt = DateTime.UtcNow});
            return Ok(_mapper.Map<ChatUserDTO>(resp));
        }

        [HttpDelete("RemoveFromChat", Name = "RemoveChatUser")]
        public async Task<IActionResult> RemoveChatUser([FromBody] ChatUserDTO item)
        {
            var user = await _userRepository.GetAsync(x => x.Id == item.UserId);
            if (user == null) return NotFound("Can't find user");
            var chat = await _chatRepository.GetAsync(x => x.Id == item.ChatId);
            if (chat == null) return NotFound("Can't find chat");
            var ChatUser = await _repository.GetAsync(x => x.UserId == item.UserId && x.ChatId == item.ChatId);
            if (ChatUser == null) return NotFound("Can'f find chat user");
            var result = await _repository.RemoveAsync(ChatUser);
            return Ok(result);
        }
    }
}
