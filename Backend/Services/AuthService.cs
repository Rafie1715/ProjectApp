using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CutiApp.DTOs;
using CutiApp.Exceptions;
using CutiApp.Models;
using CutiApp.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CutiApp.Services;

public class AuthService : IAuthService
{
    private readonly IRepository<User> _userRepo;
    private readonly IConfiguration _config;

    public AuthService(IRepository<User> userRepo, IConfiguration config)
    {
        _userRepo = userRepo;
        _config = config;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        string username = request.Username.Trim();
        User? user = await _userRepo.Query()
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null || user.Password != request.Password)
        {
            throw new UnauthorizedException("Username atau password salah.");
        }

        string token = GenerateJwtToken(user);

        return new LoginResponseDto
        {
            Token = token,
            FullName = user.FullName,
            Role = user.Role
        };
    }

    private string GenerateJwtToken(User user)
    {
        string jwtKey = _config["Jwt:Key"]!;
        string? issuer = _config["Jwt:Issuer"];
        string? audience = _config["Jwt:Audience"];
        int expireHours = int.TryParse(_config["Jwt:ExpireHours"], out int hours) ? hours : 8;

        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        Claim[] claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("fullName", user.FullName)
        };

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(expireHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
