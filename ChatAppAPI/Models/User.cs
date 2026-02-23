using ChatAppAPI.Enums;

namespace ChatAppAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Roles Role { get; set; }
        public DateTime CreatedAt { get; set; }

        // Nav
        public ICollection<Chat> ChatsCreated { get; set; }
        public ICollection<ChatUser> ChatUsers { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
