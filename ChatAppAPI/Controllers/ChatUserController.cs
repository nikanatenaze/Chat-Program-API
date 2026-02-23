using AutoMapper;
using ChatAppAPI.Hubs;
using ChatAppAPI.Models;
using ChatAppAPI.Models.ChatDTO;
using ChatAppAPI.Models.ChatUserDTO;
using ChatAppAPI.Models.UserDTO;
using ChatAppAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace ChatAppAPI.Controllers
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "4-ChatUser")]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatUserController : ControllerBase
    {
        private readonly ILogger<ChatUserController> _logger;
        private readonly IChatUserRepository _repository;
        private readonly IUserReporitory _userRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IHubContext<MainHub> _hubContext;
        private readonly IMapper _mapper;

        public ChatUserController(ILogger<ChatUserController> logger, IChatUserRepository repo, IUserReporitory userRepository, IChatRepository chatRepository, IMapper mapper, IHubContext<MainHub> hubContext)
        {
            _logger = logger;
            _repository = repo;
            _mapper = mapper;
            _userRepository = userRepository;
            _chatRepository = chatRepository;
            _hubContext = hubContext;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAll", Name = "GetAllChatUsers")]
        public async Task<IActionResult> GetAllChatUsers()
        {
            var chatUsers = await _repository.GetAllAsync();
            var result = _mapper.Map<List<ChatUserDTO>>(chatUsers);
            
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
            var (user, chat) = await GetUserAndChat(item.UserId, item.ChatId);
            if (user == null) return NotFound("Can't find user");
            if (chat == null) return NotFound("Can't find chat");
            var exists = await _repository.GetAsync(x => x.UserId == item.UserId && x.ChatId == item.ChatId);
            if (exists != null) return BadRequest("ChatUser already exits");
            var chatModel = new ChatUser { ChatId = item.ChatId, UserId = item.UserId, JoinedAt = DateTime.UtcNow };
            var resp = await _repository.AddAsync(chatModel);

            // SignalR
            await _hubContext.Clients
                .Group($"chat-users-{item.UserId}")
                .SendAsync("AddChatUser", chat);

            return Ok(_mapper.Map<ChatUserDTO>(resp));
        }

        [HttpDelete("RemoveFromChat", Name = "RemoveChatUser")]
        public async Task<IActionResult> RemoveChatUser([FromBody] ChatUserDTO item)
        {
            var (user, chat) = await GetUserAndChat(item.UserId, item.ChatId);
            if (user == null) return NotFound("Can't find user");
            if (chat == null) return NotFound("Can't find chat");
            var ChatUser = await _repository.GetAsync(x => x.UserId == item.UserId && x.ChatId == item.ChatId);
            if (ChatUser == null) return NotFound("Can'f find chat user");
            var result = await _repository.RemoveAsync(ChatUser);

            // SignalR
            await _hubContext.Clients
                .Group($"chat-users-{item.UserId}")
                .SendAsync("RemoveChatUser", chat);

            return Ok(result);
        }

        private async Task<(User user, Chat chat)> GetUserAndChat(int userId, int chatId)
        {
            var user = await _userRepository.GetAsync(x => x.Id == userId);
            var chat = await _chatRepository.GetAsync(x => x.Id == chatId);

            return (user, chat);
        }
    }
}
