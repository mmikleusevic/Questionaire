using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionaireApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedNewIndexesForQuestionAndPendingQuestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Questions_Id",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_PendingQuestions_Id",
                table: "PendingQuestions");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                column: "ConcurrencyStamp",
                value: "0ddf4ffc-f45a-487d-b871-02acee37c24a");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Id_CreatedById_ApprovedById",
                table: "Questions",
                columns: new[] { "Id", "CreatedById", "ApprovedById" });

            migrationBuilder.CreateIndex(
                name: "IX_PendingQuestions_Id_CreatedById",
                table: "PendingQuestions",
                columns: new[] { "Id", "CreatedById" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Questions_Id_CreatedById_ApprovedById",
                table: "Questions");

            migrationBuilder.DropIndex(
                name: "IX_PendingQuestions_Id_CreatedById",
                table: "PendingQuestions");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                column: "ConcurrencyStamp",
                value: "cf74fb47-bcd9-4dfb-9858-4f9c793204c1");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Id",
                table: "Questions",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_PendingQuestions_Id",
                table: "PendingQuestions",
                column: "Id");
        }
    }
}
