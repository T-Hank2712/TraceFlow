using MediatR;

namespace TraceFlow.Api.Application.Workspaces.Commands.TransferOwnership;

public record TransferOwnershipCommand(
    Ulid WorkspaceId,
    Ulid ActorUserId,
    Ulid TargetUserId
) : IRequest<TransferOwnershipResponse>;