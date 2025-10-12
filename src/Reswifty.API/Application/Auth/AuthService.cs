using MediatR;
using Reswifty.API.Application.Abstractions.Identity;
using Reswifty.API.Application.Auth.Commands;
using Reswifty.API.Application.Auth.DTOs;

namespace Reswifty.API.Application.Auth;

// public sealed class AuthService(IIdentityService identity, IJwtTokenService jwt) : IAuthService
// {
//     public async Task<AuthResultDto> LoginAsync(string email, string password, CancellationToken ct = default)
//     {
//         var userId = await identity.FindUserIdByEmailAsync(email, ct)
//                      ?? throw new InvalidCredentialsException();
//
//         var ok = await identity.CheckPasswordAsync(userId.Value, password, ct);
//         if (!ok) throw new InvalidCredentialsException();
//
//         var confirmed = await identity.IsEmailConfirmedAsync(userId.Value, ct);
//         if (!confirmed) throw new EmailNotConfirmedException();
//
//         var roles = await identity.GetRolesAsync(userId.Value, ct);
//         var accessToken = jwt.CreateAccessToken(userId.Value, email, roles);
//         var (refreshToken, expiresAt) = jwt.CreateRefreshToken(userId.Value);
//
//         return new AuthResultDto(accessToken, refreshToken, expiresAt);
//     }
// }

internal sealed class AuthService(IMediator mediator) : IAuthService
{
    public Task<AuthResult> LoginAsync(LoginCommand command, CancellationToken ct = default)
        => mediator.Send(command, ct);

    public Task<AuthResult> RefreshAsync(RefreshTokenCommand command, CancellationToken ct = default)
        => mediator.Send(command, ct);
}
