using AutoMapper;
using ChatAppAPI.Models;
using ChatAppAPI.Models.MessageDTO;
using ChatAppAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppAPI.Controllers
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "4-Messages")]
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly ILogger<MessageController> _logger;
        private readonly IMessageRepository _repository;
        private readonly IChatUserRepository _chatUserRepository;
        private readonly IMapper _mapper;

        public MessageController(ILogger<MessageController> logger, IMessageRepository repo, IMapper mapper, IChatUserRepository chatUserRepository)
        {
            _logger = logger;
            _repository = repo;
            _mapper = mapper;
            _chatUserRepository = chatUserRepository;
        }

        [HttpGet("GetAll", Name = "GetAllMessages")]
        public async Task<IActionResult> GetAllMessages()
        {
            var result = await _repository.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("GetById/{Id:int}", Name = "GetMessageById")]
        public async Task<IActionResult> GetMessageById(int Id)
        {
            var result = await _repository.GetAsync(x => x.Id == Id);
            if(result == null) return NotFound("can find message");
            return Ok(result);
        }

        [HttpPost("Create", Name = "CreateMessage")]
        public async Task<IActionResult> CreateMessage([FromBody] MessageCreateDTO dto)
        {
            // checking
            if (dto == null) return BadRequest("empty prompt");
            if (!await _chatUserRepository.IsUserInChat(dto.UserId, dto.ChatId))
                return Forbid("you are not a member of this chat");
            // adding message
            var message = _mapper.Map<Message>(dto);
            message.CreatedAt = DateTime.UtcNow;
            var result = await _repository.AddAsync(message);
            return Ok(result);
        }

        [HttpPatch("Edit", Name = "EditMessage")]
        public async Task<IActionResult> EditMessage([FromBody] MessageUpdateDTO dto)
        {
            if(dto == null) return BadRequest("empty prompt");
            var message = await _repository.GetAsync(x => x.Id == dto.Id);
            if (message == null) return BadRequest();
            if (message.UserId != dto.UserId)
                return Forbid("You cannot edit someone else's message");
            var isMember = await _chatUserRepository.IsUserInChat(dto.UserId, message.ChatId);
            if (!isMember) return Forbid("Not allowed");
            message.Content = dto.Content;
            var result = await _repository.UpdateAsync(message);
            return Ok();
        }

        [HttpDelete("Delete", Name = "DeleteMessage")]
        public async Task<IActionResult> RemoveMessage([FromBody] MessageDeleteDTO dto)
        {
            if (dto == null) return BadRequest("empty prompt");
            var message = await _repository.GetAsync(x => x.Id == dto.Id);
            if (message == null) return NotFound("can find message");
            if (message.UserId != dto.UserId)
                return Forbid("You cannot delete someone else's message");
            if (!await _chatUserRepository.IsUserInChat(dto.UserId, message.ChatId)) 
                return Forbid("Not allowed");
            var result = await _repository.RemoveAsync(message);
            return Ok(result);
        }
    }
}
