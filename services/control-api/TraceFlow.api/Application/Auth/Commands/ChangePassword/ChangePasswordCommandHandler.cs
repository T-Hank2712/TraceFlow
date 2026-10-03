namespace TraceFlow.Api.Application.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler(
    AppDbContext dbContext,
    PasswordHasher passwordHasher,
    TimeProvider timeProvider
) : IRequestHandler<ChangePasswordCommand, ChangePasswordResponse>
{

    private readonly AppDbContext _dbContext = dbContext;

    private readonly PasswordHasher _passwordHasher = passwordHasher;
    private readonly TimeProvider _timeProvider = timeProvider;

    public async Task<ChangePasswordResponse> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users
        .FirstOrDefaultAsync(
            user => user.Id == request.UserId, cancellationToken
        );

        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }

        if (user.Status != UserStatuses.Active)
        {
            throw new UnauthorizedException(
                "User account is not active.");
        }

        var currentPasswordValid = _passwordHasher.Verify(request.CurrentPassword, user.PasswordHash);
        if (!currentPasswordValid)
        {
            throw new UnauthorizedException(
                "Current password is incorrect.");
        }

        var utcNow = _timeProvider.GetUtcNow();

        var newPasswordHash = _passwordHasher.Hash(request.NewPassword);

        user.ChangePassword(newPasswordHash, utcNow);

        var activeRefreshTokens = await _dbContext.RefreshTokens
            .Where(token =>
                token.UserId == user.Id &&
                token.RevokedAt == null &&
                token.ExpiresAt > utcNow)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in activeRefreshTokens)
        {
            refreshToken.Revoke(utcNow);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ChangePasswordResponse(
            "Password changed successfully.");
    }
}
