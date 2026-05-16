using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFolioChargesAndStayNight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "folios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChargeType",
                table: "charges",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "POSReference",
                table: "charges",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "folios");

            migrationBuilder.DropColumn(
                name: "ChargeType",
                table: "charges");

            migrationBuilder.DropColumn(
                name: "POSReference",
                table: "charges");
        }
    }
}
