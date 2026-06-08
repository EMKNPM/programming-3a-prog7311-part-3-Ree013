
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GLMS.API.Models;

namespace GLMS.API.Controllers;

[ApiController]
[Route("api/auth")]

public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;

    public AuthController(IConfiguration config)
    {
        _config = config;
    }

    // POST /api/auth/login
    // Body: { "username": "admin", "password": "admin123" }
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
       
        if (dto.Username != "admin" || dto.Password != "admin123")
            return Unauthorized(new { message = "Invalid credentials." });

        var key = Encoding.ASCII.GetBytes(
            _config["Jwt:Key"] ?? "TechMoveSuperSecretKey2026ForGLMS!");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, dto.Username),
                new Claim(ClaimTypes.Role, "Admin")
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);

        return Ok(new { token = handler.WriteToken(token) });
    }
}