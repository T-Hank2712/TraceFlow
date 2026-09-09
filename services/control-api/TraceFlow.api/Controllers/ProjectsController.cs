using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraceFlow.Api.Application.Projects.Commands.CreateProject;
using TraceFlow.Api.Domain.Common.Extensions;
using TraceFlow.Api.Domain.Dtos.Projects;
using TraceFlow.Api.Application.Projects.Queries.ListProjects;
using TraceFlow.Api.Application.Projects.Queries.GetProjectById;
using TraceFlow.Api.Application.Projects.Commands.UpdateProject;
using TraceFlow.Api.Application.Projects.Commands.DeleteProject;
using TraceFlow.Api.Application.Projects.Queries.ListProjectMembers;
using TraceFlow.Api.Application.Projects.Commands.ChangeProjectMemberRole;
using TraceFlow.Api.Application.Projects.Commands.RemoveProjectMember;

namespace TraceFlow.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/workspaces/{workspaceId}/projects")]
public class ProjectsController : ControllerBase
{
    private readonly ISender _sender;

    public ProjectsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject(
        Ulid workspaceId,
        CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new CreateProjectCommand(
                workspaceId,
                userId.Value,
                request.Name,
                request.Slug,
                request.Description),
            cancellationToken);

        return StatusCode(StatusCodes.Status201Created, result);
    }
    [HttpGet]
    public async Task<IActionResult> ListProjects(
        Ulid workspaceId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new ListProjectsQuery(
                workspaceId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpGet("{projectId}")]
    public async Task<IActionResult> GetProjectById(
        Ulid workspaceId,
        Ulid projectId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new GetProjectByIdQuery(
                workspaceId,
                projectId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpPatch("{projectId}")]
    public async Task<IActionResult> UpdateProject(
        Ulid workspaceId,
        Ulid projectId,
        UpdateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new UpdateProjectCommand(
                workspaceId,
                projectId,
                userId.Value,
                request.Name,
                request.Slug,
                request.Description),
            cancellationToken);

        return Ok(result);
    }
    [HttpDelete("{projectId}")]
    public async Task<IActionResult> DeleteProject(
        Ulid workspaceId,
        Ulid projectId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new DeleteProjectCommand(
                workspaceId,
                projectId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpGet("{projectId}/members")]
    public async Task<IActionResult> ListProjectMembers(
        Ulid workspaceId,
        Ulid projectId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new ListProjectMembersQuery(
                workspaceId,
                projectId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpPatch("{projectId}/members/{memberId}/role")]
    public async Task<IActionResult> ChangeProjectMemberRole(
        Ulid workspaceId,
        Ulid projectId,
        Ulid memberId,
        ChangeProjectMemberRoleRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new ChangeProjectMemberRoleCommand(
                workspaceId,
                projectId,
                memberId,
                userId.Value,
                request.Role),
            cancellationToken);

        return Ok(result);
    }
    [HttpDelete("{projectId}/members/{memberId}")]
    public async Task<IActionResult> RemoveProjectMember(
        Ulid workspaceId,
        Ulid projectId,
        Ulid memberId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var result = await _sender.Send(
            new RemoveProjectMemberCommand(
                workspaceId,
                projectId,
                memberId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
}