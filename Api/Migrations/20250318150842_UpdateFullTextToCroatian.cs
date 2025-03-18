using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionaireApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFullTextToCroatian : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                column: "ConcurrencyStamp",
                value: "226acad3-d51e-4b79-b22e-ffd0b89f0cf8");
            
            migrationBuilder.Sql("ALTER FULLTEXT INDEX ON Questions DROP (QuestionText);", suppressTransaction: true);
            migrationBuilder.Sql("ALTER FULLTEXT INDEX ON Questions ADD (QuestionText LANGUAGE 1050);", suppressTransaction: true);

            migrationBuilder.Sql("ALTER FULLTEXT INDEX ON PendingQuestions DROP (QuestionText);", suppressTransaction: true);
            migrationBuilder.Sql("ALTER FULLTEXT INDEX ON PendingQuestions ADD (QuestionText LANGUAGE 1050);", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                column: "ConcurrencyStamp",
                value: "e77a2c7e-98f1-44a9-bbbd-2b92f7c2285c");
        }
    }
}
