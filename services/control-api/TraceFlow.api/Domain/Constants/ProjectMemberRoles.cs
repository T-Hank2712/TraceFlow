namespace TraceFlow.Api.Domain.Constants;

public static class ProjectMemberRoles
{
    public const string Manager = "manager";
    public const string Developer = "developer";
    public const string Viewer = "viewer";

    public static readonly string[] All =
    [
        Manager,
        Developer,
        Viewer
    ];
}