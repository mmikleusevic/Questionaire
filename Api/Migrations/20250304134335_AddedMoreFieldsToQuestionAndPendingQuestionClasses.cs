using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuestionaireApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedMoreFieldsToQuestionAndPendingQuestionClasses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeenAt",
                table: "UserQuestionHistory");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "PendingQuestions");
            
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Questions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Questions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ApprovedById",
                table: "Questions",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "2db072f6-3706-4996-b222-343896c40606");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Questions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "Questions",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "2db072f6-3706-4996-b222-343896c40606");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedAt",
                table: "Questions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedById",
                table: "Questions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "PendingQuestions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedById",
                table: "PendingQuestions",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "2db072f6-3706-4996-b222-343896c40606");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdatedAt",
                table: "PendingQuestions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedById",
                table: "PendingQuestions",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "0344c586-4932-4ee3-8854-65937effcbcf", null, "Admin", "ADMIN" },
                    { "e8486713-5f7d-453b-96c3-b7c3d441afb4", null, "User", "USER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Discriminator", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "2db072f6-3706-4996-b222-343896c40606", 0, "cf74fb47-bcd9-4dfb-9858-4f9c793204c1", "User", "admin@admin.com", true, false, null, "ADMIN@ADMIN.COM", "ADMIN", "AQAAAAIAAYagAAAAEEnc9IcKjqiERt+UMcv/np2qJAJtVMI6qUzqiG5HsoeCyWe0Nr/L2UZC6qmwWTdKjQ==", null, true, "b4486713-5f7d-134c-96c3-b7c3d441afb4", false, "admin" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { "0344c586-4932-4ee3-8854-65937effcbcf", "2db072f6-3706-4996-b222-343896c40606" },
                    { "e8486713-5f7d-453b-96c3-b7c3d441afb4", "2db072f6-3706-4996-b222-343896c40606" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ApprovedById",
                table: "Questions",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_CreatedById",
                table: "Questions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_LastUpdatedById",
                table: "Questions",
                column: "LastUpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PendingQuestions_CreatedById",
                table: "PendingQuestions",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PendingQuestions_LastUpdatedById",
                table: "PendingQuestions",
                column: "LastUpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_PendingQuestions_AspNetUsers_CreatedById",
                table: "PendingQuestions",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PendingQuestions_AspNetUsers_LastUpdatedById",
                table: "PendingQuestions",
                column: "LastUpdatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_AspNetUsers_ApprovedById",
                table: "Questions",
                column: "ApprovedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_AspNetUsers_CreatedById",
                table: "Questions",
                column: "CreatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Questions_AspNetUsers_LastUpdatedById",
                table: "Questions",
                column: "LastUpdatedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PendingQuestions_AspNetUsers_CreatedById",
                table: "PendingQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_PendingQuestions_AspNetUsers_LastUpdatedById",
                table: "PendingQuestions");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_AspNetUsers_ApprovedById",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_AspNetUsers_CreatedById",
                table: "Questions");

            migrationBuilder.DropForeignKey(
                name: "FK_Questions_AspNetUsers_LastUpdatedById",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_ApprovedById",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_CreatedById",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_Questions_LastUpdatedById",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_PendingQuestions_CreatedById",
                table: "PendingQuestions");

            migrationBuilder.DropIndex(
                name: "IX_PendingQuestions_LastUpdatedById",
                table: "PendingQuestions");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "0344c586-4932-4ee3-8854-65937effcbcf", "2db072f6-3706-4996-b222-343896c40606" });

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "e8486713-5f7d-453b-96c3-b7c3d441afb4", "2db072f6-3706-4996-b222-343896c40606" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "0344c586-4932-4ee3-8854-65937effcbcf");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e8486713-5f7d-453b-96c3-b7c3d441afb4");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ApprovedById",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "LastUpdatedAt",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "LastUpdatedById",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "PendingQuestions");

            migrationBuilder.DropColumn(
                name: "CreatedById",
                table: "PendingQuestions");

            migrationBuilder.DropColumn(
                name: "LastUpdatedAt",
                table: "PendingQuestions");

            migrationBuilder.DropColumn(
                name: "LastUpdatedById",
                table: "PendingQuestions");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<DateTime>(
                name: "SeenAt",
                table: "UserQuestionHistory",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "PendingQuestions",
                type: "bit",
                nullable: false,
                defaultValue: false);
            
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Questions");
        }
    }
}
