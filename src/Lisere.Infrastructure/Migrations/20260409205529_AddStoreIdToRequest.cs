using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreIdToRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StoreId",
                table: "Requests",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Requests_StoreId",
                table: "Requests",
                column: "StoreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Requests_StoreId",
                table: "Requests");

            migrationBuilder.DropColumn(
                name: "StoreId",
                table: "Requests");
        }
    }
}
