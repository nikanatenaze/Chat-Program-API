using AutoMapper;
using ChatAppAPI.Models;
using ChatAppAPI.Models.UserDTO;

namespace ChatAppAPI.Configurations
{
    public class AutoMapper : Profile
    {
        public AutoMapper()
        {
            // User
            CreateMap<User, UserDTO>();
            CreateMap<UserCreateDTO, User>();
            CreateMap<UserUpdateDTO, User>();

            // Chat
            CreateMap<Chat, ChatDTO>();
            CreateMap<ChatCreateDTO, Chat>();

            // Message
            CreateMap<Message, MessageDTO>();
            CreateMap<ChatCreateDTO, Message>();
        }
    }
}
