using TraceFlow.Api.Domain.Common;

namespace TraceFlow.Api.Domain.Entities
{
    public class User : Entity
    {
        public string Email { get; private set; } = string.Empty;
        public string UserName { get; private set; } = string.Empty;
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public string Role { get; private set; } = "user";
        public string Status { get; private set; } = "active";

        public ICollection<RefreshToken> RefreshTokens { get; private set; }
        = new List<RefreshToken>();
        public ICollection<WorkspaceMember> WorkspaceMemberships { get; set; }
            = new List<WorkspaceMember>();
        private User() {}
        public User(string Email, string UserName, string FirstName, string LastName, string PasswordHash)
        {
            this.Id = Ulid.NewUlid();
            this.Email = Email;
            this.UserName = UserName;
            this.FirstName = FirstName;
            this.LastName = LastName;
            this.PasswordHash = PasswordHash;
            this.CreatedAt = DateTime.UtcNow;
            this.UpdatedAt = DateTime.UtcNow;
        }
        public void UpdateProfile(string? UserName, string? FirstName, string? LastName)
        {
            if (!string.IsNullOrWhiteSpace(UserName))
            {
                this.UserName = UserName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(FirstName))
            {
                this.FirstName = FirstName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(LastName))
            {
                this.LastName = LastName.Trim();
            }

            this.UpdatedAt = DateTime.UtcNow;
        }
        public void ChangePassword(string PasswordHash)
        {
            this.PasswordHash = PasswordHash;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}