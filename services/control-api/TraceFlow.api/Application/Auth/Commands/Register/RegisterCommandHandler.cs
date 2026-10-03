namespace TraceFlow.Api.Application.Auth.Commands.Register;

public class RegisterCommandHandler(
    AppDbContext dbContext, 
    PasswordHasher passwordHasher
) : IRequestHandler<RegisterCommand, RegisterResponse>
{

    private readonly AppDbContext _dbContext = dbContext;

    private readonly PasswordHasher _passwordHasher = passwordHasher;

    public async Task<RegisterResponse> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken)
    {
        var emailExists = await _dbContext.Users
            .AnyAsync(
                user => user.Email == request.Email,
                cancellationToken);

        if (emailExists)
        {
            throw new ConflictException(
                "A user with this email already exists.");
        }
        var normalizedUsername = request.Username.Trim().ToLowerInvariant();

        var usernameExists = await _dbContext.Users
            .AnyAsync(user => user.NormalizedUsername == normalizedUsername, cancellationToken);

        if (usernameExists)
        {
            throw new ConflictException("Username is already taken.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = new User(
            request.Email,
            request.Username,
            request.FirstName,
            request.LastName,
            passwordHash
            );

        _dbContext.Users.Add(user);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        var reponse = new RegisterResponse(
            request.Email,
            request.Username,
            request.FirstName,
            request.LastName
        );

        return reponse;
    }
}
