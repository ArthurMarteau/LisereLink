using Lisere.Application.DTOs;

namespace Lisere.Application.Interfaces;

public interface IAuthService
{
    Task<Guid> RegisterAsync(RegisterDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
}
