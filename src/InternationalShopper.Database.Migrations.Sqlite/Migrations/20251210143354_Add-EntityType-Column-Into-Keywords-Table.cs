using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternationalShopper.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddEntityTypeColumnIntoKeywordsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Keywords",
                newName: "ListType");

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "Keywords",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "Keywords");

            migrationBuilder.RenameColumn(
                name: "ListType",
                table: "Keywords",
                newName: "Type");
        }
    }
}
