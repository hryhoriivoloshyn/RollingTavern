using Microsoft.EntityFrameworkCore.Migrations;

namespace Rolling_Tavern.Migrations
{
    public partial class SpecifyingConstraints : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Requests_AspNetUsers_ApplicationUserId",
                table: "Requests");

            migrationBuilder.DropForeignKey(
                name: "FK_Requests_Meetings_MeetingId1",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_ApplicationUserId",
                table: "Requests");

            migrationBuilder.DropIndex(
                name: "IX_Requests_MeetingId1",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "MeetingId1",
                table: "Requests");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "ApplicationUserId",
                table: "Requests",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MeetingId1",
                table: "Requests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Requests_ApplicationUserId",
                table: "Requests",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_MeetingId1",
                table: "Requests",
                column: "MeetingId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Requests_AspNetUsers_ApplicationUserId",
                table: "Requests",
                column: "ApplicationUserId",
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
    }
}
