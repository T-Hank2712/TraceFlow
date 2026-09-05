using FluentValidation;

namespace TraceFlow.Api.Application.Auth.Commands.RefreshSession;

public class RefreshSessionCommandValidator
    : AbstractValidator<RefreshSessionCommand>
{
    public RefreshSessionCommandValidator()
    {
        RuleFor(command => command.RefreshToken)
            .NotEmpty();
    }
}