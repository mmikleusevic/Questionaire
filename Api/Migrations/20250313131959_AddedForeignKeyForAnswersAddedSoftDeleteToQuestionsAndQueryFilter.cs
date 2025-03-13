using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionaireApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedForeignKeyForAnswersAddedSoftDeleteToQuestionsAndQueryFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Questions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                column: "ConcurrencyStamp",
                value: "bc88d4e2-8a6e-441b-9120-0cdd2a36ea55");
            
            migrationBuilder.Sql(@"
                ALTER TABLE Answers ADD CONSTRAINT FK_Answers_Questions 
                FOREIGN KEY (QuestionId) REFERENCES Questions(Id) 
                ON DELETE CASCADE;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Questions");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                column: "ConcurrencyStamp",
                value: "429a70c4-d5c6-4101-b94b-155f6e7091ac");
            
            migrationBuilder.Sql(@"
                ALTER TABLE Answers DROP CONSTRAINT FK_Answers_Questions;
            ");
        }
    }
}
