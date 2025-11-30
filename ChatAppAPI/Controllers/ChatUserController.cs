using AutoMapper;
using ChatAppAPI.Data;
using ChatAppAPI.Models;
using ChatAppAPI.Models.ChatUserDTO;
using ChatAppAPI.Models.UserDTO;
using ChatAppAPI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatUserController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IChatUserRepository _repository;
        private readonly IUserReporitory _userReporitory;
        private readonly IChatRepository _chatRepository;
        private readonly IMapper _mapper;

        public ChatUserController(ILogger<ChatController> logger, DataContext data, IChatUserRepository repo, IUserReporitory userReporitory, IChatRepository chatRepository, IMapper mapper)
        {
            _logger = logger;
            _repository = repo;
            _mapper = mapper;
            _userReporitory = userReporitory;
            _chatRepository = chatRepository;
        }

        [HttpGet("GetAllData", Name = "GetAllChatUsers")]
        public async Task<IActionResult> GetAllChatUsers()
        {
            var result = await _repository.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("GetUsersInChat/{Id:int}", Name = "GetUsersInChat")]
        public async Task<IActionResult> GetUsersInChat(int chatId)
        {
            var some = await _repository.GetAllAsync();
            var ids = some.Where(x => x.ChatId == chatId);
            var usersList = new List<User>();
            foreach (var i in ids) { 
                var user = await _userReporitory.GetAsync(x => x.Id == i.UserId);
                if (user != null) {
                    usersList.Add(user);
                }
            }
            var result = _mapper.Map<List<UserDTO>>(usersList); 
            return Ok(result);
        }

        [HttpGet("GetChatsOfUser/{Id:int}", Name = "GetChatsOfUser")]
        public async Task<IActionResult> GetChatsOfUser(int Id)
        {
            var some = await _repository.GetAllAsync();
            var ids = some.Where(x => x.UserId == Id);
            var chatsList = new List<Chat>();
            foreach (var i in ids)
            {
                var chat = await _chatRepository.GetAsync(x => x.Id == i.ChatId);
                if (chat != null)
                {
                    chatsList.Add(chat);
                }
            }
            var result = _mapper.Map<List<ChatDTO>>(chatsList);
            return Ok(result);
        }

        [HttpPost("AddChatUser", Name = "AddChatUser")]
        public async Task<IActionResult> AddChatUser([FromBody] ChatUserDTO item)
        {
            var user = await _userReporitory.GetAsync(x => x.Id == item.UserId);
            if (user == null) return NotFound("Can't find user");
            var chat = await _chatRepository.GetAsync(x => x.Id == item.ChatId);
            if (chat == null) return NotFound("Can't find chat");
            var exits = await _repository.GetAsync(x => x.UserId == item.UserId && x.ChatId == item.ChatId);
            if (exits != null) return BadRequest("ChatUser already exits");
            var resp = await _repository.AddAsync(new ChatUser { ChatId = item.ChatId, UserId = item.UserId, JoinedAt = DateTime.UtcNow});
            return Ok(resp);
        }

        [HttpDelete("RemoveFromChat", Name = "RemoveChatUser")]
        public async Task<IActionResult> RemoveChatUser([FromBody] ChatUserDTO item)
        {
            var user = await _userReporitory.GetAsync(x => x.Id == item.UserId);
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
