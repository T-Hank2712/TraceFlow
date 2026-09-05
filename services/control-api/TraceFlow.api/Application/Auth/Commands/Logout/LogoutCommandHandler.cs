using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Security;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Auth.Commands.Logout;

public class LogoutCommandHandler
    : IRequestHandler<LogoutCommand, LogoutResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly RefreshTokenGenerator _refreshTokenGenerator;

    public LogoutCommandHandler(
        AppDbContext dbContext,
        RefreshTokenGenerator refreshTokenGenerator)
    {
        _dbContext = dbContext;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task<LogoutResponse> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        var refreshTokenHash = RefreshTokenGenerator.Hash(
            request.RefreshToken);

        var refreshToken = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(
                token => token.TokenHash == refreshTokenHash &&
                         token.UserId == request.UserId,
                cancellationToken);

        if (refreshToken is null)
        {
            throw new UnauthorizedAccessException(
                "Invalid refresh token.");
        }

        if (refreshToken.IsActive)
        {
            refreshToken.Revoke();
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return new LogoutResponse(
            "Logged out successfully.");
    }
}