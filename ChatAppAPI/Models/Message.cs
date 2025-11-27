namespace ChatAppAPI.Models
{
    public class Message
    {
        public int Id { get; set; }

        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        // FK
        public int UserId { get; set; }
        public int ChatId { get; set; }

        // Navigation
        public User User { get; set; }
        public Chat Chat { get; set; }

    }
}
