namespace ChatAppAPI.Models
{
    public class ChatDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public bool HasPassword { get; set; }

        public DateTime CreatedAt { get; set; }
        public int CreatedByUserId { get; set; }
    }
}
