namespace ChatAppAPI.Models
{
    public class ChatUser
    {
        public int UserId { get; set; }
        public int ChatId { get; set; }

        public DateTime JoinedAt { get; set; }

        // Navigation
        public User User { get; set; }
        public Chat Chat { get; set; }
    }
}
