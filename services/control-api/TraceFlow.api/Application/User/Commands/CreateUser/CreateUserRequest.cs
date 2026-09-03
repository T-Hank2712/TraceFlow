namespace TraceFlow.Api.Dtos.User
{
    public class CreateUser
    {
        public string Email { get; init; } = string.Empty;
        public string UserName { get; init; } = string.Empty;

        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;

        public string Password { get; init; } = string.Empty;
        public string ConfirmPassword { get; init; } = string.Empty;
    }
}