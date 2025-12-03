using AutoMapper;
using ChatAppAPI.Data;
using ChatAppAPI.Models;
using ChatAppAPI.Models.ChatDTO;
using ChatAppAPI.Models.UserDTO;
using ChatAppAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppAPI.Controllers
{
    [ApiExplorerSettings(GroupName = "2-ChatsController")]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IChatRepository _repository;
        private readonly IUserReporitory _userReporitory;
        private readonly IMapper _mapper;

        public ChatController(ILogger<ChatController> logger, DataContext data, IChatRepository repo , IUserReporitory userReporitory, IMapper mapper)
        {
            _logger = logger;
            _repository = repo;
            _mapper = mapper;
            _userReporitory = userReporitory;
        }

        [HttpGet("GetAll", Name = "GetAllChats")]
        public async Task<IActionResult> GetAllChats()
        {
            var AllChats = await _repository.GetAllAsync();
            if (AllChats == null)
            {
                return NotFound("Chats doesn't found");
            }
            var Dtos = _mapper.Map<List<ChatDTO>>(AllChats);
            return Ok(Dtos);
        }


        [HttpGet("GetById/{id:int}", Name = "GetChatById")]
        public async Task<IActionResult> GetChatById(int id)
        {
            var found = await _repository.GetAsync(x => x.Id == id);
            if (found == null)
                return NotFound("Not found Chat");
            var Dto = _mapper.Map<ChatDTO>(found);
            return Ok(Dto);
        }

        [HttpPost("Create", Name = "CreateChat")]
        public async Task<IActionResult> CreateChat([FromBody] ChatCreateDTO create)
        {
            if (create == null)
                return BadRequest();
            var user = await _userReporitory.GetAsync(x => x.Id == create.CreatedByUserId);
            if (user == null) return BadRequest("Cant find user with that Id");
            var chat = _mapper.Map<Chat>(create);
            chat.CreatedAt = DateTime.UtcNow;
            await _repository.AddAsync(chat);
            return Ok(chat);
        }

        [HttpPost("ChatVerification", Name = "LoginInChat")]
        public async Task<IActionResult> LoginInChat([FromBody] ChatLoginDTO dto)
        {
            var chat = await _repository.GetAsync(x => x.Id == dto.ChatId);
            if(chat == null) return Unauthorized("cant find chat");
            if(!chat.HasPassword) return BadRequest("chat has no password");
            if(chat.Password != dto.Password) return Unauthorized("wrong password");
            return Ok(chat);
        }

        [HttpPatch("Update", Name = "UpdateChat")]
        public async Task<IActionResult> Update([FromBody] ChatUpdateDTO dto)
        {
            if (dto == null) return BadRequest();
            var aChat = await _repository.GetAsync(x => x.Id == dto.Id);
            if (aChat == null) return NotFound();
            _mapper.Map(dto, aChat);
            var result = await _repository.UpdateAsync(aChat);
            return Ok(result);
        }

        [HttpDelete("DeleteById/{Id:int}", Name = "DeleteChat")]
        public async Task<IActionResult> Delete(int Id)
        {
            var chat = await _repository.GetAsync(x => x.Id == Id);
            if (chat == null) return NotFound("Chat with that Id don't exists");
            var result = await _repository.RemoveAsync(chat);
            return Ok(result);
        }
    }
}

