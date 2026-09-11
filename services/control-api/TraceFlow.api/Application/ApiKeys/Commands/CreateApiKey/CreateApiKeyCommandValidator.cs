using FluentValidation;
using TraceFlow.Api.Domain.Constants;

namespace TraceFlow.Api.Application.ApiKeys.Commands.CreateApiKey;

public class CreateApiKeyCommandValidator : AbstractValidator<CreateApiKeyCommand>
{
    private static readonly string[] AllowedEnvironments =
    [
        ApplicationEnvironments.Development,
        ApplicationEnvironments.Staging,
        ApplicationEnvironments.Production
    ];

    private static readonly int[] AllowedExpirationPolicies =
    [
        ApiKeyExpirationPolicies.OneMonth,
        ApiKeyExpirationPolicies.ThreeMonths,
        ApiKeyExpirationPolicies.NineMonths,
        ApiKeyExpirationPolicies.TwelveMonths
    ];

    public CreateApiKeyCommandValidator()
    {
        RuleFor(command => command.Name)
            .NotEmpty()
            .MaximumLength(120);

        RuleFor(command => command.Environment)
            .NotEmpty()
            .Must(value => AllowedEnvironments.Contains(value.Trim().ToLowerInvariant()))
            .WithMessage("Invalid application environment.");

        RuleFor(command => command.ExpirationPolicy)
            .InclusiveBetween(
                ApiKeyExpirationPolicies.OneMonth,
                ApiKeyExpirationPolicies.TwelveMonths)
            .Must(value => AllowedExpirationPolicies.Contains(value))
            .WithMessage("Invalid API key expiration policy.");
    }
}