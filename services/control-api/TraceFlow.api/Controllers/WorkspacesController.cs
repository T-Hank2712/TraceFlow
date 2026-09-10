using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraceFlow.Api.Application.Workspaces.Commands.CreateWorkspace;
using TraceFlow.Api.Domain.Dtos.Workspaces;
using TraceFlow.Api.Domain.Common.Extensions;
using TraceFlow.Api.Application.Workspaces.Queries.GetWorkspaces;
using TraceFlow.Api.Application.Workspaces.Queries.GetWorkspaceById;
using TraceFlow.Api.Application.Workspaces.Commands.UpdateWorkspace;
using TraceFlow.Api.Application.Workspaces.Queries.ListMembers;
using TraceFlow.Api.Application.Workspaces.Commands.ChangeMemberRole;
using TraceFlow.Api.Application.Workspaces.Commands.DeleteWorkspace;
using TraceFlow.Api.Application.Workspaces.Commands.RemoveMember;
using TraceFlow.Api.Application.Workspaces.Commands.LeaveWorkspace;
using TraceFlow.Api.Application.Workspaces.Commands.TransferOwnership;

namespace TraceFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/workspaces")]
public class WorkspacesController : ControllerBase
{
    private readonly ISender _sender;

    public WorkspacesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> CreateWorkspace(
        CreateWorkspaceRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new CreateWorkspaceCommand(
                userId.Value,
                request.Name,
                request.Slug,
                request.Description),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetWorkspaceById),
            new { workspaceId = result.Id },
            result);
    }
    [HttpGet]
    public async Task<IActionResult> GetMyWorkspaces(CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new GetWorkspacesQuery(userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpGet("{workspaceId}")]
    public async Task<IActionResult> GetWorkspaceById(
        Ulid workspaceId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new GetWorkspaceByIdQuery(workspaceId, userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpPatch("{workspaceId}")]
    public async Task<IActionResult> UpdateWorkspace(Ulid workspaceId, UpdateWorkspaceRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }
        var result = await _sender.Send(
            new UpdateWorkspaceCommand(
                workspaceId,
                userId.Value,
                request.Name,
                request.Slug,
                request.Description),
            cancellationToken);
        return Ok(result);
    }
    [HttpGet("{workspaceId}/members")]
    public async Task<IActionResult> ListMembers(
        Ulid workspaceId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new ListMembersQuery(
                workspaceId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpPatch("{workspaceId}/members/{memberId}/role")]
    public async Task<IActionResult> ChangeMemberRole(
        Ulid workspaceId,
        Ulid memberId,
        ChangeMemberRoleRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new ChangeMemberRoleCommand(
                workspaceId,
                memberId,
                userId.Value,
                request.Role),
            cancellationToken);

        return Ok(result);
    }
    [HttpDelete("{workspaceId}")]
    public async Task<IActionResult> DeleteWorkspace(
        Ulid workspaceId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new DeleteWorkspaceCommand(
                workspaceId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpDelete("{workspaceId}/members/{memberId}")]
    public async Task<IActionResult> RemoveMember(
        Ulid workspaceId,
        Ulid memberId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new RemoveMemberCommand(
                workspaceId,
                memberId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpPost("{workspaceId}/leave")]
    public async Task<IActionResult> LeaveWorkspace(
        Ulid workspaceId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new LeaveWorkspaceCommand(
                workspaceId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpPost("{workspaceId}/transfer-ownership")]
    public async Task<IActionResult> TransferOwnership(
        Ulid workspaceId,
        TransferOwnershipRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new TransferOwnershipCommand(
                workspaceId,
                userId.Value,
                request.TargetUserId),
            cancellationToken);

        return Ok(result);
    }
}