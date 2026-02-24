using AutoMapper;
using ChatAppAPI.Hubs;
using ChatAppAPI.Models;
using ChatAppAPI.Models.MessageDTO;
using ChatAppAPI.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace ChatAppAPI.Controllers
{
    [Authorize]
    [ApiExplorerSettings(GroupName = "5-Messages")]
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly ILogger<MessageController> _logger;
        private readonly IMessageRepository _repository;
        private readonly IChatUserRepository _chatUserRepository;
        private readonly IMapper _mapper;
        private readonly IHubContext<MainHub> _hubContext;

        public MessageController(ILogger<MessageController> logger, IMessageRepository repo, IMapper mapper, IChatUserRepository chatUserRepository, IHubContext<MainHub> hubContext)
        {
            _logger = logger;
            _repository = repo;
            _mapper = mapper;
            _chatUserRepository = chatUserRepository;
            _hubContext = hubContext;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetAll", Name = "GetAllMessages")]
        public async Task<IActionResult> GetAllMessages()
        {
            var result = await _repository.GetAllAsync();
            var dtos = _mapper.Map<List<MessageDTO>>(result);

            return Ok(dtos);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("GetById/{Id:int}", Name = "GetMessageById")]
        public async Task<IActionResult> GetMessageById(int Id)
        {
            var result = await _repository.GetAsync(x => x.Id == Id);
            if(result == null) return NotFound("cannot find message");
            var dto = _mapper.Map<MessageDTO>(result);

            return Ok(dto);
        }

        [HttpPost("Create", Name = "CreateMessage")]
        public async Task<IActionResult> CreateMessage([FromBody] MessageCreateDTO dto)
        {
            // checking
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest("Message content is required");

            var admin = User.IsInRole("Admin");
            var userId = GetCurrentUserId();

            if (!await _chatUserRepository.IsUserInChat(userId, dto.ChatId) && !admin)
                return Forbid("you are not a member of this chat");

            // adding message
            var message = _mapper.Map<Message>(dto);
            message.UserId = userId;
            message.CreatedAt = DateTime.UtcNow;
            var result = await _repository.AddAsync(message);

            // SignalR
            var responseDto = _mapper.Map<MessageDTO>(result);

            await _hubContext.Clients
                .Group($"chat-{message.ChatId}")
                .SendAsync("CreateMessage", responseDto);

            return Ok(responseDto);
        }

        [HttpPatch("Edit", Name = "EditMessage")]
        public async Task<IActionResult> EditMessage([FromBody] MessageUpdateDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Content))
                return BadRequest("Message content is required");

            var message = await _repository.GetAsync(x => x.Id == dto.Id);
            if (message == null) return NotFound("message not found");

            var userId = GetCurrentUserId();
            var admin = User.IsInRole("Admin");

            if (!admin)
            {
                var isMember = await _chatUserRepository.IsUserInChat(userId, message.ChatId);
                if (!isMember)
                    return Forbid("Not allowed");
            }

            if (userId != message.UserId && !admin)
                return Forbid("You cannot edit someone else's message");

            message.Content = dto.Content;
            var result = await _repository.UpdateAsync(message);

            // signalR
            var responseDto = _mapper.Map<MessageDTO>(result);

            await _hubContext.Clients
                .Group($"chat-{message.ChatId}")
                .SendAsync("EditMessage", responseDto);
                
            return Ok(responseDto);
        }

        [HttpDelete("delete/{Id:int}", Name = "DeleteMessage")]
        public async Task<IActionResult> DeleteMessage(int Id)
        {
            if (Id == 0) return BadRequest("empty prompt");

            var message = await _repository.GetAsync(x => x.Id == Id);
            if (message == null) return NotFound("cannot find message");

            var userId = GetCurrentUserId();
            var admin = User.IsInRole("Admin");

            if (!admin)
            {
                var isMember = await _chatUserRepository.IsUserInChat(userId, message.ChatId);
                if (!isMember)
                    return Forbid("Not allowed");

                if (message.UserId != userId)
                    return Forbid("You cannot delete someone else's message");
            }

            var result = await _repository.RemoveAsync(message);

            // signalR
            var responseDto = _mapper.Map<MessageDTO>(result);

            await _hubContext.Clients
                .Group($"chat-{message.ChatId}")
                .SendAsync("RemoveMessage", responseDto);

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
