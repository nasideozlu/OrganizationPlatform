using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddVenueToBusinessPackage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VenueId",
                table: "BusinessPackages",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPackages_VenueId",
                table: "BusinessPackages",
                column: "VenueId");

            migrationBuilder.AddForeignKey(
                name: "FK_BusinessPackages_Venues_VenueId",
                table: "BusinessPackages",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BusinessPackages_Venues_VenueId",
                table: "BusinessPackages");

            migrationBuilder.DropIndex(
                name: "IX_BusinessPackages_VenueId",
                table: "BusinessPackages");

            migrationBuilder.DropColumn(
                name: "VenueId",
                table: "BusinessPackages");
        }
    }
}
