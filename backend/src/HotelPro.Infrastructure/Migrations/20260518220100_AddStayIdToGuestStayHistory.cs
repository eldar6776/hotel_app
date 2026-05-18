using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelPro.Infrastructure.Migrations
{
    public partial class AddStayIdToGuestStayHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "StayId",
                table: "GuestStayHistories",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_GuestStayHistories_stays_StayId",
                column: "StayId",
                table: "GuestStayHistories",
                principalTable: "stays",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.CreateIndex(
                name: "IX_GuestStayHistories_StayId",
                table: "GuestStayHistories",
                column: "StayId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuestStayHistories_stays_StayId",
                table: "GuestStayHistories");

            migrationBuilder.DropIndex(
                name: "IX_GuestStayHistories_StayId",
                table: "GuestStayHistories");

            migrationBuilder.DropColumn("StayId", "GuestStayHistories");
        }
    }
}
