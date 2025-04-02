using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuestionaireApi.Migrations
{
    /// <inheritdoc />
    public partial class AddedDeletedByIdDeletedAtAndDifficultyToQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Questions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedById",
                table: "Questions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Difficulty",
                table: "Questions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                column: "ConcurrencyStamp",
                value: "cc1f7e38-8eea-4047-9861-041de0cbde7b");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "DeletedById",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Difficulty",
                table: "Questions");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "2db072f6-3706-4996-b222-343896c40606",
                column: "ConcurrencyStamp",
                value: "453f2d2f-d9f4-4308-88db-e3b9c3f990e5");
        }
    }
}
