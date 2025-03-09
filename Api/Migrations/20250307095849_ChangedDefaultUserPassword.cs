using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionaireApi.Migrations
{
    /// <inheritdoc />
    public partial class ChangedDefaultUserPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "79f0a52e-db1a-4d41-a6ba-8a4c05ef46a8", "AQAAAAIAAYagAAAAEOGR7OIZBUKQavjg2sElqOw45o5Y+1E4nSu17USiT8p09MjUQqRKUL6DPCv+zeS8QA==" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "e77c3c72-0d74-48e9-9cb3-bb3119c99129", "AQAAAAIAAYagAAAAEEnc9IcKjqiERt+UMcv/np2qJAJtVMI6qUzqiG5HsoeCyWe0Nr/L2UZC6qmwWTdKjQ==" });
        }
    }
}
