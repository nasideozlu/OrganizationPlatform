using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OrganizationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddFavoriteVenues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VenueFavorites_AspNetUsers_ApplicationUserId",
                table: "VenueFavorites");
            migrationBuilder.DropForeignKey(
                name: "FK_VenueFavorites_Venues_VenueId",
                table: "VenueFavorites");

            migrationBuilder.RenameTable(
                name: "VenueFavorites",
                newName: "FavoriteVenues");
            migrationBuilder.RenameColumn(
                name: "ApplicationUserId",
                table: "FavoriteVenues",
                newName: "UserId");
            migrationBuilder.RenameIndex(
                name: "IX_VenueFavorites_ApplicationUserId_VenueId",
                table: "FavoriteVenues",
                newName: "IX_FavoriteVenues_UserId_VenueId");
            migrationBuilder.RenameIndex(
                name: "IX_VenueFavorites_VenueId",
                table: "FavoriteVenues",
                newName: "IX_FavoriteVenues_VenueId");

            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteVenues_AspNetUsers_UserId",
                table: "FavoriteVenues",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_FavoriteVenues_Venues_VenueId",
                table: "FavoriteVenues",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteVenues_AspNetUsers_UserId",
                table: "FavoriteVenues");
            migrationBuilder.DropForeignKey(
                name: "FK_FavoriteVenues_Venues_VenueId",
                table: "FavoriteVenues");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "FavoriteVenues",
                newName: "ApplicationUserId");
            migrationBuilder.RenameIndex(
                name: "IX_FavoriteVenues_UserId_VenueId",
                table: "FavoriteVenues",
                newName: "IX_VenueFavorites_ApplicationUserId_VenueId");
            migrationBuilder.RenameIndex(
                name: "IX_FavoriteVenues_VenueId",
                table: "FavoriteVenues",
                newName: "IX_VenueFavorites_VenueId");
            migrationBuilder.RenameTable(
                name: "FavoriteVenues",
                newName: "VenueFavorites");

            migrationBuilder.AddForeignKey(
                name: "FK_VenueFavorites_AspNetUsers_ApplicationUserId",
                table: "VenueFavorites",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(
                name: "FK_VenueFavorites_Venues_VenueId",
                table: "VenueFavorites",
                column: "VenueId",
                principalTable: "Venues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
