using ChatAppAPI.Enums;

namespace ChatAppAPI.Models.UserDTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public Roles Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
