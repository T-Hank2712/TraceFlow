using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Security;
using TraceFlow.Api.Infrastructure.Persistence;
using TraceFlow.Api.Domain.Entities;

namespace TraceFlow.Api.Application.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly PasswordHasher _passwordHasher;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly IConfiguration _configuration;
    private readonly RefreshTokenGenerator _refreshTokenGenerator;
    public LoginCommandHandler(AppDbContext dbContext, PasswordHasher hasher, JwtTokenGenerator tokenGenerator, IConfiguration configuration, RefreshTokenGenerator refreshTokenGenerator)
    {
        _dbContext = dbContext;
        _passwordHasher = hasher;
        _jwtTokenGenerator = tokenGenerator;
        _configuration = configuration;
        _refreshTokenGenerator = refreshTokenGenerator;
    }
    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var identifier = request.Identifier.Trim().ToLower();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(
                user => user.Email.ToLower() == identifier ||
                        user.UserName.ToLower() == identifier,
                cancellationToken);

        if (user is null)
        {
            throw new UnauthorizedAccessException("Invalid username/email or password.");
        }

        var passwordValid = _passwordHasher.Verify(request.Password, user.PasswordHash);

        if (!passwordValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid username/email or password.");
        }

        if (user.Status != "active")
        {
            throw new UnauthorizedAccessException(
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