using FluentValidation;

namespace TraceFlow.Api.Application.Auth.Commands.RefreshSession;

public class RefreshTokenCommandValidator
    : AbstractValidator<RefreshSessionCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(command => command.RefreshToken)
            .NotEmpty();
    }
}