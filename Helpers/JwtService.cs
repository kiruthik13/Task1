using HospitalManagement.Web.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HospitalManagement.Web.Helpers
{
    public class JwtService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration config, ILogger<JwtService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public string GenerateToken(User user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var secretKey   = jwtSettings["SecretKey"]   ?? throw new InvalidOperationException("JWT SecretKey not configured.");
            var issuer      = jwtSettings["Issuer"]      ?? "HospitalManagement";
            var audience    = jwtSettings["Audience"]    ?? "HospitalManagement";
            var expiryMins  = int.Parse(jwtSettings["ExpiryMinutes"] ?? "60");

            var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Name,  user.Name),
                new Claim(ClaimTypes.Role,               user.Role?.Name ?? "Patient"),
                new Claim(ClaimTypes.NameIdentifier,     user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer:             issuer,
                audience:           audience,
                claims:             claims,
                expires:            DateTime.UtcNow.AddMinutes(expiryMins),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            _logger.LogInformation("JWT token generated for user {Email}.", user.Email);
            return tokenString;
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var jwtSettings = _config.GetSection("JwtSettings");
                var secretKey   = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured.");
                var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

                var handler    = new JwtSecurityTokenHandler();
                var parameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = key,
                    ValidateIssuer           = true,
                    ValidIssuer              = jwtSettings["Issuer"],
                    ValidateAudience         = true,
                    ValidAudience            = jwtSettings["Audience"],
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero
                };

                return handler.ValidateToken(token, parameters, out _);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "JWT token validation failed.");
                return null;
            }
        }
    }
}
