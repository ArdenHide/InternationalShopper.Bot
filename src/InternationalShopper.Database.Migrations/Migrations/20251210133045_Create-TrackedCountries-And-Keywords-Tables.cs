using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace InternationalShopper.Database.Migrations
{
    /// <inheritdoc />
    public partial class CreateTrackedCountriesAndKeywordsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackedCountries",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TelegramUserId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackedCountries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrackedCountries_TelegramUsers_TelegramUserId",
                        column: x => x.TelegramUserId,
                        principalTable: "TelegramUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Keywords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Text = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    TrackedCountryId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Keywords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Keywords_TrackedCountries_TrackedCountryId",
                        column: x => x.TrackedCountryId,
                        principalTable: "TrackedCountries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Keywords_TrackedCountryId",
                table: "Keywords",
                column: "TrackedCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_TrackedCountries_TelegramUserId",
                table: "TrackedCountries",
                column: "TelegramUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Keywords");

            migrationBuilder.DropTable(
                name: "TrackedCountries");
        }
    }
}
