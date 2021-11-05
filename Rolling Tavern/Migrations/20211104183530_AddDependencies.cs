using Microsoft.EntityFrameworkCore.Migrations;

namespace Rolling_Tavern.Migrations
{
    public partial class AddDependencies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SponsorId",
                table: "Meetings");

            migrationBuilder.AddColumn<int>(
                name: "MeetingId1",
                table: "Requests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "CreatorId",
                table: "Meetings",
                type: "bigint",
                maxLength: 450,
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_MeetingId1",
                table: "Requests",
                column: "MeetingId1");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_CreatorId",
                table: "Meetings",
                column: "CreatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Users",
                table: "Meetings",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_Meetings_MeetingId1",
                table: "Requests",
                column: "MeetingId1",
                principalTable: "Meetings",
                principalColumn: "MeetingId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Users",
                table: "Meetings");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Meetings_MeetingId1",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_MeetingId1",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Meetings_CreatorId",
                table: "Meetings");

            migrationBuilder.DropColumn(
                name: "MeetingId1",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "CreatorId",
                table: "Meetings");

            migrationBuilder.AddColumn<string>(
                name: "SponsorId",
                table: "Meetings",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                defaultValue: "");
        }
    }
}
