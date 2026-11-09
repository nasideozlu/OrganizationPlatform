using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReviewStatus",
                table: "VenueRatings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ReviewStatus",
                table: "BusinessRatings",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE "BusinessRatings"
                SET "ReviewStatus" = CASE
                    WHEN "IsApproved" = TRUE THEN 1
                    ELSE 0
                END;
                """);

            migrationBuilder.Sql("""
                UPDATE "VenueRatings"
                SET "ReviewStatus" = CASE
                    WHEN "IsApproved" = TRUE THEN 1
                    ELSE 0
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "VenueRatings");

            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "BusinessRatings");
        }
    }
}
