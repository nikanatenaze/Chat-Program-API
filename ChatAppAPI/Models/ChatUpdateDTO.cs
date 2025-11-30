namespace ChatAppAPI.Models
{
    public class ChatUpdateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool HasPassword { get; set; }
        public string? Password { get; set; }
    }
}
