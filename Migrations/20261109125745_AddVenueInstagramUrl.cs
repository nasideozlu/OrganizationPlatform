using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddVenueInstagramUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstagramUrl",
                table: "Venues",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstagramUrl",
                table: "Venues");
        }
    }
}
