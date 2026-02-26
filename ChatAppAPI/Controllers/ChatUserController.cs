using AutoMapper;
using ChatAppAPI.Hubs;
using ChatAppAPI.Models;
using ChatAppAPI.Models.ChatDTO;
using ChatAppAPI.Models.ChatUserDTO;
using ChatAppAPI.Models.UserDTO;
using ChatAppAPI.Repository;
using ChatAppAPI.Services;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

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
        private readonly IUserRepository _userRepository;
        private readonly IChatRepository _chatRepository;
        private readonly IHubContext<MainHub> _hubContext;
        private readonly IMapper _mapper;

        public ChatUserController(
            ILogger<ChatUserController> logger, 
            IChatUserRepository repo, 
            IUserRepository userRepository, 
            IChatRepository chatRepository, 
            IMapper mapper,
            IHubContext<MainHub> hubContext
            )
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

        [HttpGet("GetUsersInChat/{chatId:int}", Name = "GetUsersInChat")]
        public async Task<IActionResult> GetUsersInChat(int chatId)
        {
            var currentId = GetCurrentUserId();
            var ids = await _repository.GetAllAsync(x => x.ChatId == chatId);

            var userIds = ids.Select(x => x.UserId).ToList();

            var users = await _userRepository.GetAllAsync(x => userIds.Contains(x.Id));
            if (!userIds.Contains(currentId)  && !User.IsInRole("Admin"))
            {
                return Forbid("You are not in chat");
            }

            var result = _mapper.Map<List<UserDTO>>(users);
            
            return Ok(result);
        }

        [HttpGet("GetChatsOfUser", Name = "GetChatsOfUser")]
        public async Task<IActionResult> GetChatsOfUser()
        {
            var currentUserId = GetCurrentUserId();

            var ids = await _repository.GetAllAsync(x => x.UserId == currentUserId);
            var chatIds = ids.Select(x => x.ChatId).ToList();

            var chats = await _chatRepository.GetAllAsync(x => chatIds.Contains(x.Id));
            var result = _mapper.Map<List<ChatDTO>>(chats);
            
            return Ok(result);
        }

        [HttpPost("AddChatUser", Name = "AddChatUser")]
        public async Task<IActionResult> AddChatUser([FromBody] ChatUserDTO item)
        {
            var currentId = GetCurrentUserId();

            var (user, chat) = await GetUserAndChat(item.UserId, item.ChatId);

            if (chat == null)
                return NotFound("Chat not found");

            if (user == null)
                return NotFound("User not found");

            var members = await _repository.GetAllAsync(x => x.ChatId == item.ChatId);
            var memberIds = members.Select(x => x.UserId).ToList();

            // Authorization check
            if (!memberIds.Contains(currentId) && !User.IsInRole("Admin"))
                return Forbid("You are not member of this chat!");

            // Check duplicate
            var exists = await _repository.GetAsync(x => x.UserId == item.UserId && x.ChatId == item.ChatId);
            if (exists != null)
                return BadRequest("User already in chat");

            var chatUser = new ChatUser
            {
                ChatId = item.ChatId,
                UserId = item.UserId,
                JoinedAt = DateTime.UtcNow
            };

            var resp = await _repository.AddAsync(chatUser);

            // SignalR notify added user
            await _hubContext.Clients
                .Group($"chat-users-{item.UserId}")
                .SendAsync("AddChatUser", chat);

            return Ok(_mapper.Map<ChatUserDTO>(resp));
        }

        [HttpDelete("RemoveFromChat", Name = "RemoveChatUser")]
        public async Task<IActionResult> RemoveChatUser([FromBody] ChatUserDTO item)
        {
            var currentUserId = GetCurrentUserId();

            var (user, chat) = await GetUserAndChat(item.UserId, item.ChatId);

            if (user == null)
                return NotFound("User not found");

            if (chat == null)
                return NotFound("Chat not found");

            var targetMembership = await _repository.GetAsync(x => x.UserId == item.UserId && x.ChatId == item.ChatId);
            if (targetMembership == null)
                return NotFound("User is not in this chat");

            var callerMembership = await _repository.GetAsync(x => x.UserId == currentUserId && x.ChatId == item.ChatId);

            // Authorization rules
            bool isSelf = item.UserId == currentUserId;
            bool isAdmin = User.IsInRole("Admin");

            if (!isSelf && !isAdmin)
                return Forbid("You are not allowed to remove this user"); 

            await _repository.RemoveAsync(targetMembership);

            // SignalR notify removed user
            await _hubContext.Clients
                .Group($"chat-users-{item.UserId}")
                .SendAsync("RemoveChatUser", chat);

            return Ok();
        }

        // helper methods

        private async Task<(User user, Chat chat)> GetUserAndChat(int userId, int chatId)
        {
            var user = await _userRepository.GetAsync(x => x.Id == userId);
            var chat = await _chatRepository.GetAsync(x => x.Id == chatId);

            return (user, chat);
        }

        private int GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim == null || !int.TryParse(claim.Value, out int userId))
                throw new UnauthorizedAccessException("Invalid User Id");

            return userId;
        }
    }
}
