namespace TraceFlow.Api.Domain.Entities;

public class User : Entity
{
    public string Email { get; private set; } = string.Empty;
    public string UserName { get; private set; } = string.Empty;
    public string NormalizedUsername { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = UserRoles.User;
    public string Status { get; private set; } = UserStatuses.Active;

    public ICollection<RefreshToken> RefreshTokens { get; private set; }
        = new List<RefreshToken>();
    public ICollection<WorkspaceMember> WorkspaceMemberships { get; set; }
        = new List<WorkspaceMember>();
    public ICollection<ProjectMember> ProjectMemberships { get; private set; }
        = new List<ProjectMember>();

    private User() { }

    public User(
        string email,
        string userName,
        string firstName,
        string lastName,
        string passwordHash,
        DateTimeOffset createdAt)
    {
        Id = Ulid.NewUlid();
        Email = email;
        UserName = userName;
        NormalizedUsername = NormalizeUsername(userName);
        FirstName = firstName;
        LastName = lastName;
        PasswordHash = passwordHash;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }

    public void UpdateProfile(
        string? userName,
        string? firstName,
        string? lastName,
        DateTimeOffset updatedAt)
    {
        if (!string.IsNullOrWhiteSpace(userName))
        {
            UserName = userName.Trim();
            NormalizedUsername = NormalizeUsername(userName);
        }

        if (!string.IsNullOrWhiteSpace(firstName))
        {
            FirstName = firstName.Trim();
        }

        if (!string.IsNullOrWhiteSpace(lastName))
        {
            LastName = lastName.Trim();
        }

        UpdatedAt = updatedAt;
    }

    public void ChangePassword(string passwordHash, DateTimeOffset updatedAt)
    {
        PasswordHash = passwordHash;
        UpdatedAt = updatedAt;
    }

    private static string NormalizeUsername(string username)
    {
        return username.Trim().ToLowerInvariant();
    }
}
