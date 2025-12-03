using ChatAppAPI.Models.UserDTO;

namespace ChatAppAPI.Models.AuthModels
{
    public class LoginResponseDTO
    {
        public string Token { get; set; }
        public string Email { get; set; }
    }
}
