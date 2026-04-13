using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorRequestLineOneSizePerLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestedSizes",
                table: "RequestLines");

            migrationBuilder.AddColumn<string>(
                name: "Size",
                table: "RequestLines",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Size",
                table: "RequestLines");

            migrationBuilder.AddColumn<string>(
                name: "RequestedSizes",
                table: "RequestLines",
                type: "nvarchar(200)",
                nullable: false,
                defaultValue: "");
        }
    }
}
