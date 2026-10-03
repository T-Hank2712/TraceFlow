namespace TraceFlow.Api.Application.Auth.Commands.ChangePassword;

public class ChangePasswordCommandHandler(
    AppDbContext dbContext,
    PasswordHasher passwordHasher
) : IRequestHandler<ChangePasswordCommand, ChangePasswordResponse>
{

    private readonly AppDbContext _dbContext = dbContext;

    private readonly PasswordHasher _passwordHasher = passwordHasher;

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

        var newPasswordHash = _passwordHasher.Hash(request.NewPassword);

        user.ChangePassword(newPasswordHash);

        var activeRefreshTokens = await _dbContext.RefreshTokens
            .Where(token =>
                token.UserId == user.Id &&
                token.RevokedAt == null &&
                token.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in activeRefreshTokens)
        {
            refreshToken.Revoke();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ChangePasswordResponse(
            "Password changed successfully.");
    }
}
