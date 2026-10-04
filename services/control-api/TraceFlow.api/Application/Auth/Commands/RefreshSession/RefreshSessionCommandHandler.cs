namespace TraceFlow.Api.Application.Auth.Commands.RefreshSession;

public class RefreshSessionCommandHandler(
    AppDbContext dbContext,
    JwtTokenGenerator jwtTokenGenerator,
    RefreshTokenGenerator refreshTokenGenerator,
    IOptions<JwtOptions> options,
    TimeProvider timeProvider
) : IRequestHandler<RefreshSessionCommand, RefreshSessionResponse>
{

    private readonly AppDbContext _dbContext = dbContext;

    private readonly JwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;

    private readonly RefreshTokenGenerator _refreshTokenGenerator = refreshTokenGenerator;

    private readonly JwtOptions _options = options.Value;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<RefreshSessionResponse> Handle(RefreshSessionCommand request, CancellationToken cancellationToken)
    {
        var refreshTokenHash = RefreshTokenGenerator.Hash(request.RefreshToken);

        var existingRefreshToken = await _dbContext.RefreshTokens
        .AsNoTracking()
        .Include(token => token.User)
        .FirstOrDefaultAsync(
            token => token.TokenHash == refreshTokenHash, cancellationToken
        );

        if (existingRefreshToken is null)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        var utcNow = _timeProvider.GetUtcNow();

        if (existingRefreshToken.IsRevoked)
        {
            await _dbContext.RefreshTokens
                .Where(token =>
                    token.UserId == existingRefreshToken.UserId &&
                    token.RevokedAt == null)
                .ExecuteUpdateAsync(
                    setters => setters
                        .SetProperty(token => token.RevokedAt, utcNow)
                        .SetProperty(token => token.UpdatedAt, utcNow),
                    cancellationToken);

            throw new UnauthorizedException("Refresh token reuse detected.");
        }

        if (existingRefreshToken.IsExpired(utcNow))
        {
            throw new UnauthorizedException(
                "Refresh token has expired.");
        }

        if (existingRefreshToken.User.Status != UserStatuses.Active)
        {
            throw new UnauthorizedException(
                "User account is not active.");
        }

        var accessToken = _jwtTokenGenerator.Generate(existingRefreshToken.User);
        var newRefreshToken = _refreshTokenGenerator.Generate();

        var newRefreshTokenEntity = new RefreshToken(
            existingRefreshToken.UserId,
            newRefreshToken.Hash,
            utcNow.AddDays(_options.RefreshTokenExpirationDays),
            utcNow
        );

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var revokedRows = await _dbContext.RefreshTokens
            .Where(token =>
                token.Id == existingRefreshToken.Id &&
                token.RevokedAt == null &&
                token.ExpiresAt > utcNow)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(token => token.RevokedAt, utcNow)
                    .SetProperty(token => token.UpdatedAt, utcNow),
                cancellationToken
            );

        if (revokedRows != 1)
        {
            throw new UnauthorizedException("Refresh token is no longer active.");
        }

        _dbContext.RefreshTokens.Add(newRefreshTokenEntity);

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new RefreshSessionResponse(
            accessToken.Token,
            accessToken.ExpiresAt,
            newRefreshToken.Token,
            newRefreshTokenEntity.ExpiresAt);
    }
}
