namespace TraceFlow.Api.Application.Auth.Commands.Logout;

public class LogoutCommandHandler(
    AppDbContext dbContext,
    TimeProvider timeProvider
)
    : IRequestHandler<LogoutCommand, LogoutResponse>
{

    private readonly AppDbContext _dbContext = dbContext;
    private readonly TimeProvider _timeProvider = timeProvider;

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
                "Logged out successfully.");
        }

        var utcNow = _timeProvider.GetUtcNow();

        if (refreshToken.IsActive(utcNow))
        {
            refreshToken.Revoke(utcNow);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        return new LogoutResponse(
            "Logged out successfully.");
    }
}
