namespace TraceFlow.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize]
[Route("api/v{version:apiVersion}/workspaces/{workspaceId}/projects")]
public class ProjectsController(ISender sender) : ControllerBase
{
    private readonly ISender _sender = sender;


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
    [HttpGet("~/api/v{version:apiVersion}/projects/{projectId}")]
    public async Task<IActionResult> GetProjectById(
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
                projectId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpPatch("~/api/v{version:apiVersion}/projects/{projectId}")]
    public async Task<IActionResult> UpdateProject(
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
                projectId,
                userId.Value,
                request.Name,
                request.Slug,
                request.Description),
            cancellationToken);

        return Ok(result);
    }
    [HttpDelete("~/api/v{version:apiVersion}/projects/{projectId}")]
    public async Task<IActionResult> DeleteProject(
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
                projectId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpGet("~/api/v{version:apiVersion}/projects/{projectId}/members")]
    public async Task<IActionResult> ListProjectMembers(
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
                projectId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpPatch("~/api/v{version:apiVersion}/members/{memberId}")]
    public async Task<IActionResult> ChangeProjectMemberRole(
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
                memberId,
                userId.Value,
                request.Role),
            cancellationToken);

        return Ok(result);
    }
    [HttpDelete("~/api/v{version:apiVersion}/members/{memberId}")]
    public async Task<IActionResult> RemoveProjectMember(
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
                memberId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
    [HttpPost("~/api/v{version:apiVersion}/projects/{projectId}/leave")]
    public async Task<IActionResult> LeaveProject(
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
                projectId,
                userId.Value),
            cancellationToken);

        return Ok(result);
    }
}
