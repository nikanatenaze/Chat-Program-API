namespace ChatAppAPI.Models
{
    public class Chat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool HasPassword { get; set; }
        public string ?Password { get; set; }
        public string ?ChatImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        // FK
        public int CreatedByUserId { get; set; }
        // Navigation
        public User CreatedByUser { get; set; }
        public ICollection<ChatUser> ChatUsers { get; set; }
        public ICollection<Message> Messages { get; set; }
    }
}
