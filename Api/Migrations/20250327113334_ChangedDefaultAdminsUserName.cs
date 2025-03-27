using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionaireApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangedDefaultAdminsUserName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                columns: new[] { "ConcurrencyStamp", "NormalizedUserName", "UserName" },
                values: new object[] { "453f2d2f-d9f4-4308-88db-e3b9c3f990e5", "ADMIN", "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                columns: new[] { "ConcurrencyStamp", "NormalizedUserName", "UserName" },
                values: new object[] { "f92d75a8-7e1a-4965-ab7d-6ba43c5276bc", "ADMIN@ADMIN.COM", "admin@admin.com" });
        }
    }
}
