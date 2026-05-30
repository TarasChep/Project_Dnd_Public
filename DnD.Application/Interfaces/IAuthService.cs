using DnD.Application.DTOs;

namespace DnD.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto model);
    Task<AuthResponseDto> LoginAsync(LoginDto model);
}
