using FluentValidation;

namespace TraceFlow.Api.Application.Auth.Commands.Logout;

public class LogoutCommandValidator
    : AbstractValidator<LogoutCommand>
{
    public LogoutCommandValidator()
    {
        RuleFor(command => command.UserId)
            .NotEmpty();

        RuleFor(command => command.RefreshToken)
            .NotEmpty();
    }
}