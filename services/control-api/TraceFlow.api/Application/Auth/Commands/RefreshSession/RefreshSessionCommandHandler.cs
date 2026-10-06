namespace TraceFlow.Api.Application.Auth.Commands.RefreshSession;

public class RefreshSessionCommandHandler(
    AppDbContext dbContext,
    JwtTokenGenerator jwtTokenGenerator,
    RefreshTokenGenerator refreshTokenGenerator,
    IOptions<JwtOptions> options,
    TimeProvider timeProvider
) : IRequestHandler<RefreshSessionCommand, RefreshSessionResponse>
{
    private const string InvalidRefreshTokenMessage = "Invalid refresh token.";

    private readonly AppDbContext _dbContext = dbContext;
    private readonly JwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
    private readonly RefreshTokenGenerator _refreshTokenGenerator = refreshTokenGenerator;
    private readonly JwtOptions _options = options.Value;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<RefreshSessionResponse> Handle(
        RefreshSessionCommand request,
        CancellationToken cancellationToken)
    {
        var refreshTokenHash =
            _refreshTokenGenerator.Hash(request.RefreshToken);

        var utcNow = _timeProvider.GetUtcNow();

        var tokenSnapshot = await _dbContext.RefreshTokens
            .AsNoTracking()
            .Include(token => token.User)
            .FirstOrDefaultAsync(
                token => token.TokenHash == refreshTokenHash,
                cancellationToken);

        if (tokenSnapshot is null)
        {
            throw new UnauthorizedException(InvalidRefreshTokenMessage);
        }

        if (tokenSnapshot.IsRevoked)
        {
            await RevokeActiveTokensForUserAsync(
                tokenSnapshot.UserId,
                utcNow,
                cancellationToken);

            throw new UnauthorizedException(InvalidRefreshTokenMessage);
        }

        if (tokenSnapshot.IsExpired(utcNow))
        {
            throw new UnauthorizedException(InvalidRefreshTokenMessage);
        }

        if (tokenSnapshot.User.Status != UserStatuses.Active)
        {
            throw new UnauthorizedException(InvalidRefreshTokenMessage);
        }

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var existingRefreshToken = await _dbContext.RefreshTokens
                .Include(token => token.User)
                .FirstOrDefaultAsync(
                    token =>
                        token.Id == tokenSnapshot.Id &&
                        token.RevokedAt == null &&
                        token.ExpiresAt > utcNow,
                    cancellationToken);

            if (existingRefreshToken is null)
            {
                throw new UnauthorizedException(InvalidRefreshTokenMessage);
            }

            if (existingRefreshToken.User.Status != UserStatuses.Active)
            {
                throw new UnauthorizedException(InvalidRefreshTokenMessage);
            }

            existingRefreshToken.Revoke(utcNow);

            var accessToken =
                _jwtTokenGenerator.Generate(existingRefreshToken.User);

            var newRefreshToken =
                _refreshTokenGenerator.Generate();

            var newRefreshTokenEntity = new RefreshToken(
                existingRefreshToken.UserId,
                newRefreshToken.Hash,
                utcNow.AddDays(_options.RefreshTokenExpirationDays),
                utcNow);

            _dbContext.RefreshTokens.Add(newRefreshTokenEntity);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return new RefreshSessionResponse(
                accessToken.Token,
                accessToken.ExpiresAt,
                newRefreshToken.Token,
                newRefreshTokenEntity.ExpiresAt);
        });
    }

    private async Task RevokeActiveTokensForUserAsync(
        Ulid userId,
        DateTimeOffset utcNow,
        CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction =
                await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            var activeTokens = await _dbContext.RefreshTokens
                .Where(token =>
                    token.UserId == userId &&
                    token.RevokedAt == null &&
                    token.ExpiresAt > utcNow)
                .ToListAsync(cancellationToken);

            foreach (var token in activeTokens)
            {
                token.Revoke(utcNow);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        });
    }
}