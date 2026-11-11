using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddContentRejectionReason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Venues",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "VenuePhotos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "BusinessProfiles",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "BusinessPhotos",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "VenuePhotos");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "BusinessProfiles");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "BusinessPhotos");
        }
    }
}
