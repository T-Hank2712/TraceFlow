namespace TraceFlow.Api.Application.Auth.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler(
    AppDbContext dbContext
) : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
{

    private readonly AppDbContext _dbContext = dbContext;

    public async Task<CurrentUserResponse> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken
    )
    {
        var user = await _dbContext.Users
        .AsNoTracking()
        .Where(user => user.Id == request.UserId)
        .Select(user => new CurrentUserResponse(
            user.Id,
            user.Email,
            user.UserName,
            user.FirstName,
            user.LastName,
            user.Role,
            user.Status
        ))
        .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            throw new NotFoundException("User not found");
        }
        return user;
    }
}
