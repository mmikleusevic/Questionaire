using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionaireApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangedIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserQuestionHistory_UserId_QuestionId",
                table: "UserQuestionHistory");

            migrationBuilder.DropIndex(
                name: "IX_Questions_Id_CreatedById",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_QuestionCategories_QuestionId_CategoryId",
                table: "QuestionCategories");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Questions",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValue: new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                column: "ConcurrencyStamp",
                value: "70054d32-15c0-4c03-a929-9119f7047b0f");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuestionHistory_UserId_QuestionId",
                table: "UserQuestionHistory",
                columns: new[] { "UserId", "QuestionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Difficulty",
                table: "Questions",
                column: "Difficulty");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserQuestionHistory_UserId_QuestionId",
                table: "UserQuestionHistory");

            migrationBuilder.DropIndex(
                name: "IX_Questions_Difficulty",
                table: "Questions");

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "Questions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                column: "ConcurrencyStamp",
                value: "cc1f7e38-8eea-4047-9861-041de0cbde7b");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuestionHistory_UserId_QuestionId",
                table: "UserQuestionHistory",
                columns: new[] { "UserId", "QuestionId" });

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Id_CreatedById",
                table: "Questions",
                columns: new[] { "Id", "CreatedById" });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionCategories_QuestionId_CategoryId",
                table: "QuestionCategories",
                columns: new[] { "QuestionId", "CategoryId" });
        }
    }
}
