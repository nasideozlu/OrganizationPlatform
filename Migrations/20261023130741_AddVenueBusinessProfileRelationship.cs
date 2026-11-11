using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddVenueBusinessProfileRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BusinessProfileId",
                table: "Venues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Venues_BusinessProfileId",
                table: "Venues",
                column: "BusinessProfileId");

            migrationBuilder.AddForeignKey(
                name: "FK_Venues_BusinessProfiles_BusinessProfileId",
                table: "Venues",
                column: "BusinessProfileId",
                principalTable: "BusinessProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Venues_BusinessProfiles_BusinessProfileId",
                table: "Venues");

            migrationBuilder.DropIndex(
                name: "IX_Venues_BusinessProfileId",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "BusinessProfileId",
                table: "Venues");
        }
    }
}
