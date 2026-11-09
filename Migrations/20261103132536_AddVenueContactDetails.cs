using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddVenueContactDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Venues",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "Venues",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "Venues");

            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "Venues");
        }
    }
}
