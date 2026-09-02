namespace TraceFlow.api.Dtos.User
{
    public class CreateUser
    {
        public string Email { get; init; } = string.Empty;

        public string DisplayName { get; init; } = string.Empty;

        public string Password { get; init; } = string.Empty;
    }
}