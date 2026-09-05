namespace TraceFlow.Api.Domain.Dtos;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword
);