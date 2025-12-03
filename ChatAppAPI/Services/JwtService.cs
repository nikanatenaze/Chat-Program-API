using ChatAppAPI.Data;
using ChatAppAPI.Models.AuthDTO;
using ChatAppAPI.Models.AuthModels;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ChatAppAPI.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;
        private readonly DataContext _db;

        public JwtService(IConfiguration config, DataContext db)
        {
            _db = db;
            _config = config;
        }

        public async Task<LoginResponseDTO> Authenticate(LoginRequestDTO login)
        {
            var jwt = _config.GetSection("Jwt");

            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var key = _config["Jwt:Key"];
            var tokenValidityMins = _config["Jwt:ExpiresInMinutes"];

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Email, login.Email),
                }),
                Issuer = issuer,
                Audience = audience,
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(tokenValidityMins)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);

            return new LoginResponseDTO { Email = login.Email, Token = accessToken };
        }
    }
}