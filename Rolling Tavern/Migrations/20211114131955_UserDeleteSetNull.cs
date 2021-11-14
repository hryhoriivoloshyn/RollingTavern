using Microsoft.EntityFrameworkCore.Migrations;

namespace Rolling_Tavern.Migrations
{
    public partial class UserDeleteSetNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Users",
                table: "Meetings");

            migrationBuilder.AlterColumn<long>(
                name: "CreatorId",
                table: "Meetings",
                type: "bigint",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldMaxLength: 450);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Users",
                table: "Meetings",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meetings_Users",
                table: "Meetings");

            migrationBuilder.AlterColumn<long>(
                name: "CreatorId",
                table: "Meetings",
                type: "bigint",
                maxLength: 450,
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Meetings_Users",
                table: "Meetings",
                column: "CreatorId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
