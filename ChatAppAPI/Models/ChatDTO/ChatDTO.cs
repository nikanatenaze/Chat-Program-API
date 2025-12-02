namespace ChatAppAPI.Models.ChatDTO
{
    public class ChatDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public bool HasPassword { get; set; }
        public string? Password { get; set; }

        public DateTime CreatedAt { get; set; }
        public int CreatedByUserId { get; set; }
    }
}
