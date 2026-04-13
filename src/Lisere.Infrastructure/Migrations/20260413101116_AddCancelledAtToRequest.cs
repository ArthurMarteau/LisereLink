using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lisere.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCancelledAtToRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CancelledAt",
                table: "Requests",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CancelledAt",
                table: "Requests");
        }
    }
}
