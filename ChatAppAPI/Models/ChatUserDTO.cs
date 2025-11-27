namespace ChatAppAPI.Models
{
    public class ChatUserDTO
    {
        public int UserId { get; set; }
        public int ChatId { get; set; }

        public DateTime JoinedAt { get; set; }
    }
}
