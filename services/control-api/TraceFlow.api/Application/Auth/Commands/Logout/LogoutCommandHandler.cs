namespace TraceFlow.Api.Application.Auth.Commands.Logout;

public class LogoutCommandHandler(
    AppDbContext dbContext
)
    : IRequestHandler<LogoutCommand, LogoutResponse>
{

    private readonly AppDbContext _dbContext = dbContext;

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
            throw new UnauthorizedException(
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
