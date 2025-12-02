using AutoMapper;
using ChatAppAPI.Models;
using ChatAppAPI.Models.MessageDTO;
using ChatAppAPI.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ChatAppAPI.Controllers
{
    [ApiExplorerSettings(GroupName = "4-MessagesController")]
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

        [HttpGet("GetById{Id:int}", Name = "GetMessageById")]
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
            if (dto == null) return BadRequest();
            var isMember = await _chatUserRepository.GetAllAsync(cu =>
            cu.ChatId == dto.ChatId && cu.UserId == dto.UserId);

            if (isMember == null || !isMember.Any())
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
            if(dto == null) return BadRequest();
            var message = await _repository.GetAsync(x => x.Id == dto.Id);
            if (message == null) return BadRequest();
            message.Content = dto.Content;
            var result = _repository.UpdateAsync(message);
            return Ok();
        }

        [HttpDelete("Delete/{id:int}", Name = "DeleteMessage")]
        public async Task<IActionResult> RemoveMessage(int id)
        {
            var message = await _repository.GetAsync(x => x.Id == id);
            if (message == null) return NotFound("can find message");
            var result = await _repository.RemoveAsync(message);
            return Ok(result);
        }
    }
}
