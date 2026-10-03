namespace TraceFlow.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/workspaces/{workspaceId}/projects")]
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
    [HttpPost("{projectId}/leave")]
    public async Task<IActionResult> LeaveProject(
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
            new LeaveProjectCommand(
                workspaceId,
                projectId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
}
