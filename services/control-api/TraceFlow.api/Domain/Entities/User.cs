using TraceFlow.api.Domain.Common;

namespace TraceFlow.Api.Domain.Entities
{
    public class User : Entity
    {
        public string Email { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "viewer";
        public string Status { get; set; } = "active";

        // public ICollection<WorkspaceMember> WorkspaceMemberships { get; set; }
        //     = new List<WorkspaceMember>();
    }
}