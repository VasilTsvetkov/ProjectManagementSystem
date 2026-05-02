using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagementSystem.Migrations
{
    /// <inheritdoc />
    public partial class IntegrationRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_AspNetUsers_CreatedById",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CreatedById",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "Projects");

            migrationBuilder.AddColumn<string>(
                name: "ReporterId",
                table: "Tasks",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CreatorId",
                table: "Projects",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Number",
                table: "Projects",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_ReporterId",
                table: "Tasks",
                column: "ReporterId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatorId",
                table: "Projects",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_AspNetUsers_CreatorId",
                table: "Projects",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_AspNetUsers_ReporterId",
                table: "Tasks",
                column: "ReporterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_AspNetUsers_CreatorId",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_AspNetUsers_ReporterId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_ReporterId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CreatorId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ReporterId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Number",
                table: "Projects");

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Projects",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatedById",
                table: "Projects",
                column: "CreatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_AspNetUsers_CreatedById",
                table: "Projects",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
