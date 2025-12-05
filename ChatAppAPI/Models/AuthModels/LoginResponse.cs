using ChatAppAPI.Models.UserDTO;

namespace ChatAppAPI.Models.AuthModels
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public string Email { get; set; }
    }
}
