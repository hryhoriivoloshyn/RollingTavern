using Microsoft.EntityFrameworkCore.Migrations;

namespace Rolling_Tavern.Migrations
{
    public partial class MeetingAgeProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MinimalAge",
                table: "Meetings",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MinimalAge",
                table: "Meetings");
        }
    }
}
