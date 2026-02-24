using AutoMapper;
using ChatAppAPI.Data;
using ChatAppAPI.Models;
using ChatAppAPI.Models.AuthDTO;
using ChatAppAPI.Models.AuthModels;
using ChatAppAPI.Models.UserDTO;
using ChatAppAPI.Repository;
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
        private readonly IUserRepository _reporitory;
        private readonly IMapper _mapper;

        public JwtService(IConfiguration config, IUserRepository reporitory, IMapper mapper)
        {
            _reporitory = reporitory;
            _mapper = mapper;
            _config = config;
        }

        public async Task<LoginResponse> Authenticate(User request)
        {

            if (request == null)
                throw new Exception("Invalid credentials");

            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var key = _config["Jwt:Key"];
            var tokenValidityMins = _config["Jwt:ExpiresInMinutes"] ?? "60";

            var claims = new List<Claim> {
                new Claim(JwtRegisteredClaimNames.Sub, request.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, request.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, request.Email),
                new Claim(ClaimTypes.Role, request.Role.ToString()),
                new Claim(ClaimTypes.Name, request.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = issuer,
                Audience = audience,
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(tokenValidityMins)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var accessToken = tokenHandler.WriteToken(securityToken);

            var account = _mapper.Map<UserDTO>(request);
            

            return new LoginResponse { User = account, Token = accessToken };
        }
    }
}