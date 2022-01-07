using Microsoft.EntityFrameworkCore.Migrations;

using System;

namespace HR.PersonalTimetable.Api.Persistence.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PersonalTimetable",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(25)", maxLength: 25, nullable: false),
                    InstituteName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ElementType = table.Column<int>(type: "int", nullable: false),
                    ElementId = table.Column<int>(type: "int", nullable: false),
                    ElementName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SchoolYearId = table.Column<int>(type: "int", nullable: true),
                    IsVisible = table.Column<bool>(type: "bit", nullable: false),
                    DateCreated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "sysdatetimeoffset()"),
                    DateLastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PersonalTimetable", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PersonalTimetable_UserName",
                table: "PersonalTimetable",
                column: "UserName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PersonalTimetable");
        }
    }
}
