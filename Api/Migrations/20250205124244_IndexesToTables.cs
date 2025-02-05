using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionaireApi.Migrations
{
    /// <inheritdoc />
    public partial class IndexesToTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserQuestionHistory",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_UserQuestionHistory_UserId_QuestionId",
                table: "UserQuestionHistory",
                columns: new[] { "UserId", "QuestionId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserQuestionHistory_UserId_QuestionId",
                table: "UserQuestionHistory");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserQuestionHistory",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
