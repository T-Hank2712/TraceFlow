using MediatR;
using Microsoft.EntityFrameworkCore;
using TraceFlow.Api.Infrastructure.Persistence;

namespace TraceFlow.Api.Application.Users.Queries.GetUser;

public class GetUserQueryHandler
    : IRequestHandler<GetUserQuery, UserResponse?>
{
    private readonly AppDbContext _dbContext;

    public GetUserQueryHandler(
        AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UserResponse?> Handle(
        GetUserQuery request,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(user => user.Id == request.Id)
            .Select(user => new UserResponse(
                user.Id,
                user.Email,
                user.UserName,
                user.FirstName,
                user.LastName,
                user.CreatedAt,
                user.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);
    }
}