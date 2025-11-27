using AutoMapper;
using ChatAppAPI.Models;

namespace ChatAppAPI.Configurations
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            // User
            CreateMap<User, UserDTO>();
            CreateMap<ChatCreateDTO, User>();

            // Chat
            CreateMap<Chat, ChatDTO>();
            CreateMap<ChatCreateDTO, Chat>();

            // Message
            CreateMap<Message, MessageDTO>();
            CreateMap<ChatCreateDTO, Message>();
        }
    }
}
