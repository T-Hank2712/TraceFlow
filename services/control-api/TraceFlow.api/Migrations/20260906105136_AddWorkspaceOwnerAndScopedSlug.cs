using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TraceFlow.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkspaceOwnerAndScopedSlug : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Workspaces_Slug",
                table: "Workspaces");

            migrationBuilder.AddColumn<string>(
                name: "OwnerUserId",
                table: "Workspaces",
                type: "text",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Workspaces" w
                SET "OwnerUserId" = wm."UserId"
                FROM "WorkspaceMembers" wm
                WHERE wm."WorkspaceId" = w."Id"
                AND wm."Role" = 'owner';
            """);

            migrationBuilder.AlterColumn<string>(
                name: "OwnerUserId",
                table: "Workspaces",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_OwnerUserId_Slug",
                table: "Workspaces",
                columns: new[] { "OwnerUserId", "Slug" },
                unique: true,
                filter: "\"Status\" = 'active'");

            migrationBuilder.AddForeignKey(
                name: "FK_Workspaces_Users_OwnerUserId",
                table: "Workspaces",
                column: "OwnerUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workspaces_Users_OwnerUserId",
                table: "Workspaces");

            migrationBuilder.DropIndex(
                name: "IX_Workspaces_OwnerUserId_Slug",
                table: "Workspaces");

            migrationBuilder.DropColumn(
                name: "OwnerUserId",
                table: "Workspaces");

            migrationBuilder.CreateIndex(
                name: "IX_Workspaces_Slug",
                table: "Workspaces",
                column: "Slug",
                unique: true);
        }
    }
}
