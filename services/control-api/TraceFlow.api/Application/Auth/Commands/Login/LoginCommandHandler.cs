namespace TraceFlow.Api.Application.Auth.Commands.Login;

public class LoginCommandHandler(
    AppDbContext dbContext,
    PasswordHasher passwordHasher,
    JwtTokenGenerator jwtTokenGenerator,
    IOptions<JwtOptions> options,
    RefreshTokenGenerator refreshTokenGenerator,
    TimeProvider timeProvider
) : IRequestHandler<LoginCommand, LoginResponse>
{

    private readonly AppDbContext _dbContext = dbContext;

    private readonly PasswordHasher _passwordHasher = passwordHasher;

    private readonly JwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;

    private readonly JwtOptions _options = options.Value;

    private readonly RefreshTokenGenerator _refreshTokenGenerator = refreshTokenGenerator;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var identifier = request.Identifier.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(
                user => user.Email == identifier ||
                        user.NormalizedUsername == identifier,
                cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedException("Invalid username/email or password.");
        }

        var passwordVerificationResult = _passwordHasher.VerifyDetailed(
            request.Password,
            user.PasswordHash);

        if (passwordVerificationResult == PasswordVerificationResult.Failed)
        {
            throw new UnauthorizedException(
                "Invalid username/email or password.");
        }

        if (user.Status != UserStatuses.Active)
        {
            throw new NotFoundException(
                "Invalid username/email or password.");
        }

        var accessToken = _jwtTokenGenerator.Generate(user);
        var refreshToken = _refreshTokenGenerator.Generate();

        var refreshTokenExpirationDays = _options.RefreshTokenExpirationDays;

        var utcNow = _timeProvider.GetUtcNow();

        if (passwordVerificationResult == PasswordVerificationResult.SuccessRehashNeeded)
        {
            var newPasswordHash = _passwordHasher.Hash(request.Password);
            user.ChangePassword(newPasswordHash, utcNow);
        }

        var refreshTokenEntity = new RefreshToken(user.Id, refreshToken.Hash, utcNow.AddDays(refreshTokenExpirationDays), utcNow);

        _dbContext.RefreshTokens.Add(refreshTokenEntity);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new LoginResponse(
            accessToken.Token,
            accessToken.ExpiresAt,
            refreshToken.Token,
            refreshTokenEntity.ExpiresAt,
            new LoginUserResponse(
                user.Id,
                user.Email,
                user.UserName,
                user.FirstName,
                user.LastName,
                user.Role));
    }
}
