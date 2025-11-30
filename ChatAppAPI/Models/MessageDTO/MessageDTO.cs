namespace ChatAppAPI.Models.MessageDTO
{
    public class MessageDTO
    {
        public int Id { get; set; }

        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }
        public int ChatId { get; set; }
    }
}
