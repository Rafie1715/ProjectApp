using CutiApp.DTOs;

namespace CutiApp.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);
}

