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
    [Authorize]
    [ApiExplorerSettings(GroupName = "3-Chats")]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ILogger<ChatController> _logger;
        private readonly IChatRepository _repository;
        private readonly IUserReporitory _userReporitory;
        private readonly IMessageRepository _messageRepository;
        private readonly IMapper _mapper;

        public ChatController(ILogger<ChatController> logger, DataContext data, IChatRepository repo , IUserReporitory userReporitory, IMessageRepository messageRepository,IMapper mapper)
        {
            _logger = logger;
            _repository = repo;
            _mapper = mapper;
            _userReporitory = userReporitory;
            _messageRepository = messageRepository;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAll", Name = "GetAllChats")]
        public async Task<IActionResult> GetAllChats()
        {
            var AllChats = await _repository.GetAllAsync();
            var Dtos = _mapper.Map<List<ChatDTO>>(AllChats);

            return Ok(Dtos);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetById/{id:int}", Name = "GetChatById")]
        public async Task<IActionResult> GetChatById(int id)
        {
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
            var user = await _userReporitory.GetAsync(x => x.Id == create.CreatedByUserId);
            if (user == null) return BadRequest("Cant find user with that Id");
            var chat = _mapper.Map<Chat>(create);
            chat.HasPassword = !string.IsNullOrEmpty(create.Password);
            chat.CreatedAt = DateTime.UtcNow;
            var result = await _repository.AddAsync(chat);
            var resultDto = _mapper.Map<ChatDTO>(result);

            return Ok(resultDto);
        }

        [HttpPost("ChatVerification", Name = "LoginInChat")]
        public async Task<IActionResult> LoginInChat([FromBody] ChatLoginDTO dto)
        {
            var chat = await _repository.GetAsync(x => x.Id == dto.ChatId);
            if(chat == null) return Unauthorized("cant find chat");
            if(!chat.HasPassword) return BadRequest("chat has no password");
            if(chat.Password != dto.Password) return Unauthorized("wrong password");
            var cDto = _mapper.Map<ChatDTO>(chat);

            return Ok(cDto);
        }

        [HttpPatch("Update", Name = "UpdateChat")]
        public async Task<IActionResult> Update([FromBody] ChatUpdateDTO dto)
        {
            if (dto == null) return BadRequest();
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
            var chat = await _repository.GetAsync(x => x.Id == Id);
            if (chat == null) return NotFound("Chat with that Id don't exists");
            var result = await _repository.RemoveAsync(chat);

            return Ok(result);
        }
    }
}

