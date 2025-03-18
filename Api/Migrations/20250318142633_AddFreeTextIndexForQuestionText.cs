using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionaireApi.Migrations
{
    /// <inheritdoc />
    public partial class AddFreeTextIndexForQuestionText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                column: "ConcurrencyStamp",
                value: "e77a2c7e-98f1-44a9-bbbd-2b92f7c2285c");
            
            migrationBuilder.Sql("CREATE FULLTEXT CATALOG ftCatalog AS DEFAULT;", suppressTransaction: true);
            
            migrationBuilder.Sql(
                @"
                CREATE FULLTEXT INDEX ON Questions(QuestionText)
                KEY INDEX PK_Questions
                WITH STOPLIST = SYSTEM;",
                suppressTransaction: true
            );
            
            migrationBuilder.Sql(
                @"
                CREATE FULLTEXT INDEX ON PendingQuestions(QuestionText)
                KEY INDEX PK_PendingQuestions
                WITH STOPLIST = SYSTEM;",
                suppressTransaction: true
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                column: "ConcurrencyStamp",
                value: "bc88d4e2-8a6e-441b-9120-0cdd2a36ea55");
            
            migrationBuilder.Sql("DROP FULLTEXT INDEX ON Questions;", suppressTransaction: true);
            migrationBuilder.Sql("DROP FULLTEXT INDEX ON PendingQuestions;", suppressTransaction: true);
            migrationBuilder.Sql("DROP FULLTEXT CATALOG ftCatalog;", suppressTransaction: true);
        }
    }
}
