using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Application.Common.Exceptions;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Users.Commands.UpdateProfile;

public class UpdateMyProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UpdateProfileResponse>
{
    private readonly AppDbContext _dbContext;
    public UpdateMyProfileCommandHandler(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<UpdateProfileResponse> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(
            user => user.Id == request.UserId, cancellationToken
        );

        if (user is null)
        {
            throw new NotFoundException("User not found.");
        }
        if (user.Status != "active")
        {
            throw new UnauthorizedAccessException(
                "User account is not active.");
        }
        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            var normalizedUserName = request.UserName.Trim();

            var userNameExists = await _dbContext.Users
                .AnyAsync(
                    user => user.UserName == normalizedUserName &&
                            user.Id != request.UserId,
                    cancellationToken);

            if (userNameExists)
            {
                throw new InvalidOperationException(
                    "Username is already taken.");
            }
        }

        user.UpdateProfile(
            request.UserName,
            request.FirstName,
            request.LastName
        );

        await _dbContext.SaveChangesAsync(cancellationToken);
        return new UpdateProfileResponse(
            user.Id,
            user.Email,
            user.UserName,
            user.FirstName,
            user.LastName,
            user.Role,
            user.Status);
    }
}