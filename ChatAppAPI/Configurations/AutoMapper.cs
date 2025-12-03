using AutoMapper;
using ChatAppAPI.Models;
using ChatAppAPI.Models.MessageDTO;
using ChatAppAPI.Models.UserDTO;
using ChatAppAPI.Models.ChatUserDTO;
using ChatAppAPI.Models.ChatDTO;
using ChatAppAPI.Models.AuthDTO;

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
            CreateMap<RegisterRequest, User>();
                

            // Chat
            CreateMap<Chat, ChatDTO>();
            CreateMap<ChatCreateDTO, Chat>();
            CreateMap<ChatUpdateDTO, Chat>()
                .ForMember(dest => dest.CreatedByUserId, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore());

            // ChatUser
            CreateMap<ChatUserDTO, ChatUser>();
            CreateMap<ChatUser, ChatUserDTO>();

            // Message
            CreateMap<Message, MessageDTO>();
            CreateMap<MessageCreateDTO, Message>();
            CreateMap<MessageUpdateDTO, Message>();
        }
    }
}
