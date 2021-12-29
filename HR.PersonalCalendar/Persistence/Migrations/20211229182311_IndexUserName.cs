using Microsoft.EntityFrameworkCore.Migrations;

namespace HR.PersonalCalendar.Persistence.Migrations
{
    public partial class IndexUserName : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PersonalTimetable_UserName",
                table: "PersonalTimetable",
                column: "UserName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PersonalTimetable_UserName",
                table: "PersonalTimetable");
        }
    }
}
