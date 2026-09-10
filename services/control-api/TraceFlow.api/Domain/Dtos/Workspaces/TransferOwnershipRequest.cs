namespace TraceFlow.Api.Domain.Dtos.Workspaces;

public record TransferOwnershipRequest(
    Ulid TargetUserId);