using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionaireApi.Migrations
{
    /// <inheritdoc />
    public partial class RemovedAllPendingTablesChangedQuestionProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Questions_Id_CreatedById_ApprovedById",
                table: "Questions");
            
            migrationBuilder.DropIndex(
                name: "IX_Questions_ApprovedById",
                table: "Questions");

            migrationBuilder.AlterColumn<bool>(
                name: "IsApproved",
                table: "Questions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                column: "ConcurrencyStamp",
                value: "f92d75a8-7e1a-4965-ab7d-6ba43c5276bc");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Id_CreatedById",
                table: "Questions",
                columns: new[] { "Id", "CreatedById" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Questions_Id_CreatedById",
                table: "Questions");

            migrationBuilder.AlterColumn<bool>(
                name: "IsApproved",
                table: "Questions",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                column: "ConcurrencyStamp",
                value: "6976178d-7fb1-44bd-a246-4fefe7ac841b");

            migrationBuilder.CreateIndex(
                name: "IX_Questions_Id_CreatedById_ApprovedById",
                table: "Questions",
                columns: new[] { "Id", "CreatedById", "ApprovedById", "QuestionText" });
            
            migrationBuilder.CreateIndex(
                name: "IX_Questions_ApprovedById",
                table: "Questions",
                columns: new[] { "ApprovedById" });
        }
    }
}
