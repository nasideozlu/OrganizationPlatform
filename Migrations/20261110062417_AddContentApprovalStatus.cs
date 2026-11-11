using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddContentApprovalStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "Venues",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "VenuePhotos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "BusinessProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "BusinessPhotos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE "BusinessProfiles"
                SET "ApprovalStatus" = 1;

                UPDATE "BusinessPhotos"
                SET "ApprovalStatus" = 1;

                UPDATE "Venues"
                SET "ApprovalStatus" = 1;

                UPDATE "VenuePhotos"
                SET "ApprovalStatus" = 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "VenuePhotos");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "BusinessProfiles");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "BusinessPhotos");
        }
    }
}
