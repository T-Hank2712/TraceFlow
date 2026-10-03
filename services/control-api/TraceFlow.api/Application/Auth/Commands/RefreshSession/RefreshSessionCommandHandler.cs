namespace TraceFlow.Api.Application.Auth.Commands.RefreshSession;

public class RefreshSessionCommandHandler(
    AppDbContext dbContext,
    JwtTokenGenerator jwtTokenGenerator,
    RefreshTokenGenerator refreshTokenGenerator,
    IConfiguration configuration,
    TimeProvider timeProvider
) : IRequestHandler<RefreshSessionCommand, RefreshSessionResponse>
{

    private readonly AppDbContext _dbContext = dbContext;

    private readonly JwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;

    private readonly RefreshTokenGenerator _refreshTokenGenerator = refreshTokenGenerator;

    private readonly IConfiguration _configuration = configuration;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<RefreshSessionResponse> Handle(RefreshSessionCommand request, CancellationToken cancellationToken)
    {
        var refreshTokenHash = RefreshTokenGenerator.Hash(request.RefreshToken);

        var existingRefreshToken = await _dbContext.RefreshTokens
        .Include(token => token.User)
        .FirstOrDefaultAsync(
            token => token.TokenHash == refreshTokenHash, cancellationToken
        );

        if (existingRefreshToken is null)
        {
            throw new UnauthorizedException("Invalid refresh token.");
        }

        var utcNow = _timeProvider.GetUtcNow();

        if (!existingRefreshToken.IsActive(utcNow))
        {
            throw new UnauthorizedException(
                "Refresh token is no longer active.");
        }

        if (existingRefreshToken.User.Status != UserStatuses.Active)
        {
            throw new UnauthorizedException(
                "User account is not active.");
        }

        existingRefreshToken.Revoke(utcNow);

        var accessToken = _jwtTokenGenerator.Generate(existingRefreshToken.User);
        var newRefreshToken = _refreshTokenGenerator.Generate();
        var refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "30");

        var newRefreshTokenEntity = new RefreshToken(
            existingRefreshToken.UserId,
            newRefreshToken.Hash,
            utcNow.AddDays(refreshTokenExpirationDays),
            utcNow);

        _dbContext.RefreshTokens.Add(newRefreshTokenEntity);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RefreshSessionResponse(
            accessToken.Token,
            accessToken.ExpiresAt,
            newRefreshToken.Token,
            newRefreshTokenEntity.ExpiresAt);
    }
}
