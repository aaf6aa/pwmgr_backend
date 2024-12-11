using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using pwmgr_backend.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace pwmgr_backend.Helpers
{
    public static class JwtHelper
    {
        public static string GenerateJwtToken(User user, IConfiguration configuration)
        {
            IConfigurationSection jwtSettings = configuration.GetSection("Jwt") ?? throw new Exception("Jwt section is missing in appsettings.json");

            var key = Convert.FromBase64String(configuration["Jwt:Key"] ?? throw new Exception("Jwt key secret is not set"));

            var tokenHandler = new JwtSecurityTokenHandler();

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                }),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpiresInMinutes"] ?? throw new Exception("Jwt ExpireInMinutes is missing in appsettings.json"))),
                Issuer = jwtSettings["Issuer"] ?? throw new Exception("Jwt Issuer is missing in appsettings.json"),
                Audience = jwtSettings["Audience"] ?? throw new Exception("Jwt Audience is missing in appsettings.json"),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}
