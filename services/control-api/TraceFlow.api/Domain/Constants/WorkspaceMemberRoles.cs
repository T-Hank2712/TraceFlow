namespace TraceFlow.Api.Domain.Constants;

public static class WorkspaceMemberRoles
{
    public const string Owner = "owner";
    public const string Admin = "admin";
    public const string Member = "member";

    public static readonly string[] All =
    [
        Owner,
        Admin,
        Member
    ];

    public static readonly string[] Assignable =
    [
        Admin,
        Member
    ];
}