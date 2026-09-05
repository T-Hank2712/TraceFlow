using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Application.Common.Security;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, ChangePasswordResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly PasswordHasher _passwordHasher;
    public ChangePasswordCommandHandler(AppDbContext dbContext, PasswordHasher passwordHasher)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
    } 
    public async Task<ChangePasswordResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
        .FirstOrDefaultAsync(
            user => user.Id == request.UserId, cancellationToken
        );

        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        if (user.Status != "active")
        {
            throw new UnauthorizedAccessException(
                "User account is not active.");
        }

        var currentPasswordValid = _passwordHasher.Verify(request.CurrentPassword, user.PasswordHash);
        if (!currentPasswordValid)
        {
            throw new UnauthorizedAccessException(
                "Current password is incorrect.");
        }

        var newPasswordHash = _passwordHasher.Hash(request.NewPassword);

        user.ChangePassword(newPasswordHash);

        var activeRefreshTokens = await _dbContext.RefreshTokens
            .Where(token =>
                token.UserId == user.Id &&
                token.RevokedAt == null &&
                token.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in activeRefreshTokens)
        {
            refreshToken.Revoke();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ChangePasswordResponse(
            "Password changed successfully.");
    }
}