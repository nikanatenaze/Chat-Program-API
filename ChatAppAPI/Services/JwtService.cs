using AutoMapper;
using ChatAppAPI.Data;
using ChatAppAPI.Models;
using ChatAppAPI.Models.AuthDTO;
using ChatAppAPI.Models.AuthModels;
using ChatAppAPI.Models.TokenModels;
using ChatAppAPI.Models.UserDTO;
using ChatAppAPI.Repository;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ChatAppAPI.Services
{
    public class JwtService
    {
        private readonly IConfiguration _config;
        private readonly IUserRepository _reporitory;
        private readonly IMapper _mapper;
        private readonly DataContext _context;

        public JwtService(
            IConfiguration config, 
            IUserRepository reporitory, 
            IMapper mapper,
            DataContext data
            )
        {
            _reporitory = reporitory;
            _mapper = mapper;
            _config = config;
            _context = data;
        }

        // splited Authenticate
        public async Task<LoginResponse> Authenticate(User user)
        {
            if (user == null)
                throw new Exception("Invalid credentials");

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            });

            await _context.SaveChangesAsync();

            var account = _mapper.Map<UserDTO>(user);

            return new LoginResponse
            {
                User = account,
                Token = accessToken,
                RefreshToken = refreshToken
            };
        }


        // Generate refresh token
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        // generate access token
        public string GenerateAccessToken(User user)
        {
            var issuer = _config["Jwt:Issuer"];
            var audience = _config["Jwt:Audience"];
            var key = _config["Jwt:Key"];
            var tokenValidityMins = _config["Jwt:ExpiresInMinutes"] ?? "60";

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = issuer,
                Audience = audience,
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(tokenValidityMins)),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                    SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }

        // geting data from old expired token
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_config["Jwt:Key"])
                ),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256))
                throw new SecurityTokenException("Invalid token");

            return principal;
        }

        // refresch old token
        public async Task<TokenResult> Refresh(string accessToken, string refreshToken)
        {
            var principal = GetPrincipalFromExpiredToken(accessToken);

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                throw new SecurityTokenException("Invalid token");

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(x =>
                    x.UserId == int.Parse(userId) &&
                    x.Token == refreshToken &&
                    !x.IsRevoked);

            if (storedToken == null || storedToken.ExpiryDate <= DateTime.UtcNow)
                throw new SecurityTokenException("Invalid refresh token");

            storedToken.IsRevoked = true;

            var user = await _context.Users.FindAsync(int.Parse(userId));

            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshToken,
                ExpiryDate = DateTime.UtcNow.AddDays(7)
            });

            await _context.SaveChangesAsync();

            return new TokenResult
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }
    }
}