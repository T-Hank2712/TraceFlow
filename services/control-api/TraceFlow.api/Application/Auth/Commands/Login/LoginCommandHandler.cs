namespace TraceFlow.Api.Application.Auth.Commands.Login;

public class LoginCommandHandler(
    AppDbContext dbContext,
    PasswordHasher passwordHasher,
    JwtTokenGenerator jwtTokenGenerator,
    IConfiguration configuration,
    RefreshTokenGenerator refreshTokenGenerator
) : IRequestHandler<LoginCommand, LoginResponse>
{

    private readonly AppDbContext _dbContext = dbContext;

    private readonly PasswordHasher _passwordHasher = passwordHasher;

    private readonly JwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;

    private readonly IConfiguration _configuration = configuration;

    private readonly RefreshTokenGenerator _refreshTokenGenerator = refreshTokenGenerator;

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var identifier = request.Identifier.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(
                user => user.Email.ToLower() == identifier ||
                        user.NormalizedUsername == identifier,
                cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedException("Invalid username/email or password.");
        }

        var passwordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!passwordValid)
        {
            throw new UnauthorizedException(
                "Invalid username/email or password.");
        }

        if (user.Status != UserStatuses.Active)
        {
            throw new NotFoundException(
                "User account is not found.");
        }

        var accessToken = _jwtTokenGenerator.Generate(user);
        var refreshToken = _refreshTokenGenerator.Generate();

        var refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "30");

        var refreshTokenEntity = new RefreshToken(user.Id, refreshToken.Hash, DateTime.UtcNow.AddDays(refreshTokenExpirationDays));

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
