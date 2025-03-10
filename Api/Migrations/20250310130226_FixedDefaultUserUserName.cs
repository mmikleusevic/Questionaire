using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionaireApi.Migrations
{
    /// <inheritdoc />
    public partial class FixedDefaultUserUserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                columns: new[] { "ConcurrencyStamp", "NormalizedUserName", "UserName" },
                values: new object[] { "429a70c4-d5c6-4101-b94b-155f6e7091ac", "ADMIN@ADMIN.COM", "admin@admin.com" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                columns: new[] { "ConcurrencyStamp", "NormalizedUserName", "UserName" },
                values: new object[] { "79f0a52e-db1a-4d41-a6ba-8a4c05ef46a8", "ADMIN", "admin" });
        }
    }
}
