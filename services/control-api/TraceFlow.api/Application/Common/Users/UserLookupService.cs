using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Domain.Constants;
using TraceFlow.Api.Domain.Entities;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Common.Users;

public class UserLookupService
{
    private readonly AppDbContext _dbContext;

    public UserLookupService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User> GetActiveInviteTargetAsync(
        string identifier,
        Ulid actorUserId,
        CancellationToken cancellationToken)
    {
        var normalizedIdentifier = identifier.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(
                user =>
                    user.Email.ToLower() == normalizedIdentifier ||
                    user.NormalizedUsername == normalizedIdentifier,
                cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User to invite not found.");
        }

        if (user.Status != UserStatuses.Active)
        {
            throw new ConflictException("Cannot invite inactive user.");
        }

        if (user.Id == actorUserId)
        {
            throw new ConflictException("You cannot invite yourself.");
        }

        return user;
    }
}