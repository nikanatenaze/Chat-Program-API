namespace ChatAppAPI.Models.ChatDTO
{
    public class ChatCreateDTO
    {
        public string Name { get; set; }
        public bool HasPassword { get; set; }
        public string? Password { get; set; }
        public int CreatedByUserId { get; set; }
    }
}
