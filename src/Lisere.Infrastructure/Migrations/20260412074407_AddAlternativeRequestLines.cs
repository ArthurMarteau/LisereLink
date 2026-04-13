using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlternativeRequestLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlternativeArticleId",
                table: "RequestLines");

            migrationBuilder.DropColumn(
                name: "AlternativeColorOrPrint",
                table: "RequestLines");

            migrationBuilder.DropColumn(
                name: "AlternativeSizes",
                table: "RequestLines");

            migrationBuilder.DropColumn(
                name: "AlternativeStockOverride",
                table: "RequestLines");

            migrationBuilder.CreateTable(
                name: "AlternativeRequestLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArticleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ArticleName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ArticleColorOrPrint = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ArticleBarcode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RequestedSizes = table.Column<string>(type: "nvarchar(500)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    StockOverride = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlternativeRequestLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlternativeRequestLines_Requests_RequestId",
                        column: x => x.RequestId,
                        principalTable: "Requests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlternativeRequestLines_RequestId",
                table: "AlternativeRequestLines",
                column: "RequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlternativeRequestLines");

            migrationBuilder.AddColumn<Guid>(
                name: "AlternativeArticleId",
                table: "RequestLines",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlternativeColorOrPrint",
                table: "RequestLines",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AlternativeSizes",
                table: "RequestLines",
                type: "nvarchar(200)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AlternativeStockOverride",
                table: "RequestLines",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
