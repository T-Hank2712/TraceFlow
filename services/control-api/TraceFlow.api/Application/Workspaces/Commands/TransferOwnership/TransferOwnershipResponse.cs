namespace TraceFlow.Api.Application.Workspaces.Commands.TransferOwnership;

public record TransferOwnershipResponse(
    Ulid WorkspaceId,
    Ulid TargetUserId,
    string TargetRole,
    DateTime UpdatedAt);