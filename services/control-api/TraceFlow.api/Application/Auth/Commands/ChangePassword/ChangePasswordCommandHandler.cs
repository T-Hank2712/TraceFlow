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

        var strategy = _dbContext.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            var user = await _dbContext.Users
            .FirstOrDefaultAsync(
                user => user.Id == request.UserId, cancellationToken
            );

            if (user is null || user.Status != UserStatuses.Active)
            {
                throw new NotFoundException("User not found.");
            }

            var currentPasswordValid = _passwordHasher.Verify(request.CurrentPassword, user.PasswordHash);

            if (!currentPasswordValid)
            {
                throw new UnauthorizedException(
                    "Current password is incorrect.");
            }

            var utcNow = _timeProvider.GetUtcNow();

            var newPasswordHash = _passwordHasher.Hash(request.NewPassword);

            await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

            user.ChangePassword(newPasswordHash, utcNow);

            var activeRefreshTokens = await _dbContext.RefreshTokens
                .Where(token =>
                    token.UserId == user.Id &&
                    token.RevokedAt == null &&
                    token.ExpiresAt > utcNow)
                .ToListAsync(cancellationToken);

            foreach (var token in activeRefreshTokens)
            {
                token.Revoke(utcNow);
            }
                    
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new ChangePasswordResponse(
                "Password changed successfully.");
        });
    }
}
