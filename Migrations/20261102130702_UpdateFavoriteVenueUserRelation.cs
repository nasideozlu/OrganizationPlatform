using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OrganizationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFavoriteVenueUserRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteVenues_AspNetUsers_UserId",
                table: "FavoriteVenues");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "FavoriteVenues",
                newName: "ApplicationUserId");

            migrationBuilder.RenameIndex(
                name: "IX_FavoriteVenues_UserId_VenueId",
                table: "FavoriteVenues",
                newName: "IX_FavoriteVenues_ApplicationUserId_VenueId");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteVenues_AspNetUsers_ApplicationUserId",
                table: "FavoriteVenues",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteVenues_AspNetUsers_ApplicationUserId",
                table: "FavoriteVenues");

            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "FavoriteVenues",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_FavoriteVenues_ApplicationUserId_VenueId",
                table: "FavoriteVenues",
                newName: "IX_FavoriteVenues_UserId_VenueId");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteVenues_AspNetUsers_UserId",
                table: "FavoriteVenues",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
