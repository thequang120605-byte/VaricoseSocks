using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using VaricoseSocks.Application.Interfaces;
using VaricoseSocks.Domain.Entities;

namespace VaricoseSocks.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;

    public JwtService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    private string GetSecretKey()
    {
        var envSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
        if (!string.IsNullOrWhiteSpace(envSecret)) return envSecret;

        var configuredSecret = _configuration["JwtSettings:Secret"];
        if (!string.IsNullOrWhiteSpace(configuredSecret) && configuredSecret.Length >= 32)
        {
            return configuredSecret;
        }

        return "VaricoseSocks_Medical_Secure_Secret_Key_2026_Minimum_32_Bytes_Long_JWT_Secret!";
    }

    public string GenerateToken(User user, IEnumerable<string> roles)
    {
        var secret = GetSecretKey();
        var issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? _configuration["JwtSettings:Issuer"] ?? "VaricoseSocks.API";
        var audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? _configuration["JwtSettings:Audience"] ?? "VaricoseSocks.Clients";
        var expiryHours = int.TryParse(_configuration["JwtSettings:ExpiryInHours"], out var hours) ? hours : 24;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(expiryHours),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
