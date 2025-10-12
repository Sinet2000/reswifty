using Dexlaris.Core.Models;
using Reswifty.API.Application.Auth.Commands;
using Reswifty.API.Application.Auth.DTOs;
using Reswifty.API.Contracts.Auth.Requests;

namespace Reswifty.API.Application.Abstractions.Identity;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(LoginCommand command, CancellationToken ct = default);

    // Task<Result<AuthResult>> RefreshAsync(RefreshTokenCommand command, CancellationToken ct = default);
    // Task<Result> LogoutAsync(Guid userId, CancellationToken ct = default);
    // Task<Result> RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}
