using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OrganizationPlatform.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessCategoryFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BusinessFilterDefinitions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Code = table.Column<string>(type: "text", nullable: false),
                    DataType = table.Column<int>(type: "integer", nullable: false),
                    Unit = table.Column<string>(type: "text", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    BusinessCategoryId = table.Column<int>(type: "integer", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessFilterDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessFilterDefinitions_BusinessCategories_BusinessCatego~",
                        column: x => x.BusinessCategoryId,
                        principalTable: "BusinessCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessFilterOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    BusinessFilterDefinitionId = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessFilterOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessFilterOptions_BusinessFilterDefinitions_BusinessFil~",
                        column: x => x.BusinessFilterDefinitionId,
                        principalTable: "BusinessFilterDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessProfileFilterValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessProfileId = table.Column<int>(type: "integer", nullable: false),
                    BusinessFilterDefinitionId = table.Column<int>(type: "integer", nullable: false),
                    NumericValue = table.Column<decimal>(type: "numeric", nullable: true),
                    TextValue = table.Column<string>(type: "text", nullable: true),
                    BooleanValue = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessProfileFilterValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessProfileFilterValues_BusinessFilterDefinitions_Busin~",
                        column: x => x.BusinessFilterDefinitionId,
                        principalTable: "BusinessFilterDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusinessProfileFilterValues_BusinessProfiles_BusinessProfil~",
                        column: x => x.BusinessProfileId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BusinessProfileFilterOptions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BusinessProfileId = table.Column<int>(type: "integer", nullable: false),
                    BusinessFilterOptionId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessProfileFilterOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BusinessProfileFilterOptions_BusinessFilterOptions_Business~",
                        column: x => x.BusinessFilterOptionId,
                        principalTable: "BusinessFilterOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BusinessProfileFilterOptions_BusinessProfiles_BusinessProfi~",
                        column: x => x.BusinessProfileId,
                        principalTable: "BusinessProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BusinessFilterDefinitions_BusinessCategoryId_Code",
                table: "BusinessFilterDefinitions",
                columns: new[] { "BusinessCategoryId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessFilterOptions_BusinessFilterDefinitionId_Value",
                table: "BusinessFilterOptions",
                columns: new[] { "BusinessFilterDefinitionId", "Value" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProfileFilterOptions_BusinessFilterOptionId",
                table: "BusinessProfileFilterOptions",
                column: "BusinessFilterOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProfileFilterOptions_BusinessProfileId_BusinessFilt~",
                table: "BusinessProfileFilterOptions",
                columns: new[] { "BusinessProfileId", "BusinessFilterOptionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProfileFilterValues_BusinessFilterDefinitionId",
                table: "BusinessProfileFilterValues",
                column: "BusinessFilterDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_BusinessProfileFilterValues_BusinessProfileId_BusinessFilte~",
                table: "BusinessProfileFilterValues",
                columns: new[] { "BusinessProfileId", "BusinessFilterDefinitionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BusinessProfileFilterOptions");

            migrationBuilder.DropTable(
                name: "BusinessProfileFilterValues");

            migrationBuilder.DropTable(
                name: "BusinessFilterOptions");

            migrationBuilder.DropTable(
                name: "BusinessFilterDefinitions");
        }
    }
}
