namespace ChatAppAPI.Models
{
    public class JoinChatDTO
    {
        public int ChatId { get; set; }
        public int UserId { get; set; }

        public string? Password { get; set; }
    }
}
