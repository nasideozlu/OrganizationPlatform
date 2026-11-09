using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OrganizationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddVenueRatingsAndReviewReplies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BusinessReply",
                table: "BusinessRatings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "BusinessReplyAt",
                table: "BusinessRatings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "BusinessRatings",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "VenueRatings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ApplicationUserId = table.Column<string>(type: "text", nullable: false),
                    VenueId = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    BusinessReply = table.Column<string>(type: "text", nullable: true),
                    BusinessReplyAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueRatings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VenueRatings_Venues_VenueId",
                        column: x => x.VenueId,
                        principalTable: "Venues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VenueRatings_ApplicationUserId_VenueId",
                table: "VenueRatings",
                columns: new[] { "ApplicationUserId", "VenueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VenueRatings_VenueId",
                table: "VenueRatings",
                column: "VenueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VenueRatings");

            migrationBuilder.DropColumn(
                name: "BusinessReply",
                table: "BusinessRatings");

            migrationBuilder.DropColumn(
                name: "BusinessReplyAt",
                table: "BusinessRatings");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "BusinessRatings");
        }
    }
}
