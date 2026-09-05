using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TraceFlow.Api.Application.Common.Security;
using TraceFlow.Api.Domain.Entities;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Auth.Commands.RefreshSession;

public class RefreshSessionCommandHandler : IRequestHandler<RefreshSessionCommand, RefreshSessionResponse>
{
    private readonly AppDbContext _dbContext;
    private readonly JwtTokenGenerator _jwtTokenGenerator;
    private readonly RefreshTokenGenerator _refreshTokenGenerator;
    private readonly IConfiguration _configuration;
    public RefreshSessionCommandHandler(AppDbContext dbContext, JwtTokenGenerator jwtTokenGenerator, RefreshTokenGenerator refreshTokenGenerator, IConfiguration configuration)
    {
        _dbContext = dbContext;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
        _configuration = configuration;
    }
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
            throw new UnauthorizedAccessException("Invalid refresh token.");
        }


        if (!existingRefreshToken.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Refresh token is no longer active.");
        }

        if (existingRefreshToken.User.Status != "active")
        {
            throw new UnauthorizedAccessException(
                "User account is not active.");
        }

        existingRefreshToken.Revoke();

        var accessToken = _jwtTokenGenerator.Generate(existingRefreshToken.User);
        var newRefreshToken = _refreshTokenGenerator.Generate();
        var refreshTokenExpirationDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "30");

        var newRefreshTokenEntity = new RefreshToken(
            existingRefreshToken.UserId,
            newRefreshToken.Hash,
            DateTime.UtcNow.AddDays(refreshTokenExpirationDays));

                _dbContext.RefreshTokens.Add(newRefreshTokenEntity);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new RefreshSessionResponse(
            accessToken.Token,
            accessToken.ExpiresAt,
            newRefreshToken.Token,
            newRefreshTokenEntity.ExpiresAt);
    }
}