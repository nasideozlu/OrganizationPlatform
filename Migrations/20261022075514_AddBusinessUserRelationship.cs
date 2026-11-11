using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessUserRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "BusinessProfiles",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProfiles_ApplicationUserId",
                table: "BusinessProfiles",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessProfiles_AspNetUsers_ApplicationUserId",
                table: "BusinessProfiles",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessProfiles_AspNetUsers_ApplicationUserId",
                table: "BusinessProfiles");

            migrationBuilder.DropIndex(
                name: "IX_BusinessProfiles_ApplicationUserId",
                table: "BusinessProfiles");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "BusinessProfiles");
        }
    }
}
