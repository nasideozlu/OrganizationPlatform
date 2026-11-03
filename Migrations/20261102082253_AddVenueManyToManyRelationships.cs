using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddVenueManyToManyRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "BusinessCampaignVenues",
                columns: table => new
                {
                    BusinessCampaignsId = table.Column<int>(type: "integer", nullable: false),
                    VenuesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessCampaignVenues", x => new { x.BusinessCampaignsId, x.VenuesId });
                    table.ForeignKey(
                        name: "FK_BusinessCampaignVenues_BusinessCampaigns_BusinessCampaignsId",
                        column: x => x.BusinessCampaignsId,
                        principalTable: "BusinessCampaigns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusinessCampaignVenues_Venues_VenuesId",
                        column: x => x.VenuesId,
                        principalTable: "Venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessPackageVenues",
                columns: table => new
                {
                    BusinessPackagesId = table.Column<int>(type: "integer", nullable: false),
                    VenuesId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessPackageVenues", x => new { x.BusinessPackagesId, x.VenuesId });
                    table.ForeignKey(
                        name: "FK_BusinessPackageVenues_BusinessPackages_BusinessPackagesId",
                        column: x => x.BusinessPackagesId,
                        principalTable: "BusinessPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusinessPackageVenues_Venues_VenuesId",
                        column: x => x.VenuesId,
                        principalTable: "Venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessCampaignVenues_VenuesId",
                table: "BusinessCampaignVenues",
                column: "VenuesId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessPackageVenues_VenuesId",
                table: "BusinessPackageVenues",
                column: "VenuesId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessCampaignVenues");

            migrationBuilder.DropTable(
                name: "BusinessPackageVenues");

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
    }
}
