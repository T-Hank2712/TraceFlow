using MediatR;

namespace TraceFlow.Api.Application.Auth.Commands.ChangePassword;

public record ChangePasswordCommand(
    Ulid UserId,
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
) : IRequest<ChangePasswordResponse>;