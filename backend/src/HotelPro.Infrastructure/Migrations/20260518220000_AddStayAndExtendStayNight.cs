using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelPro.Infrastructure.Migrations
{
    public partial class AddStayAndExtendStayNight : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HotelId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuestId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    FolioId = table.Column<Guid>(type: "uuid", nullable: true),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    BookingRoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    TariffId = table.Column<Guid>(type: "uuid", nullable: true),
                    CheckInDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckedInBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CheckedOutBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CheckedOutAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsCheckedOut = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRegistrationPrinted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsReservationLink = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsFromConfirmedReservation = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAccommodationPaid = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    GuestCategory = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    DiscountPercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false, defaultValue: 0m),
                    DiscountReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TaxOverride = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    StayNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ServiceNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PaymentNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stays_guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stays_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stays_folios_FolioId",
                        column: x => x.FolioId,
                        principalTable: "folios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stays_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_stays_booking_rooms_BookingRoomId",
                        column: x => x.BookingRoomId,
                        principalTable: "booking_rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_stays_RoomId_IsCheckedOut",
                table: "stays",
                columns: new[] { "RoomId", "IsCheckedOut" });

            migrationBuilder.CreateIndex(
                name: "IX_stays_GuestId_IsCheckedOut",
                table: "stays",
                columns: new[] { "GuestId", "IsCheckedOut" });

            migrationBuilder.CreateIndex(
                name: "IX_stays_FolioId",
                table: "stays",
                column: "FolioId");

            migrationBuilder.AddColumn<Guid>(
                name: "StayId",
                table: "stay_nights",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                table: "stay_nights",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercent",
                table: "stay_nights",
                type: "decimal(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "stay_nights",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "stay_nights",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosedAt",
                table: "stay_nights",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.RenameColumn(
                name: "RoomPrice",
                table: "stay_nights",
                newName: "TariffAmount");

            migrationBuilder.AddForeignKey(
                name: "FK_stay_nights_stays_StayId",
                column: "StayId",
                table: "stay_nights",
                principalTable: "stays",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_stay_nights_rooms_RoomId",
                column: "RoomId",
                table: "stay_nights",
                principalTable: "rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateIndex(
                name: "IX_stay_nights_StayId_Date",
                table: "stay_nights",
                columns: new[] { "StayId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_stay_nights_RoomId_Date",
                table: "stay_nights",
                columns: new[] { "RoomId", "Date" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_stay_nights_stays_StayId",
                table: "stay_nights");

            migrationBuilder.DropForeignKey(
                name: "FK_stay_nights_rooms_RoomId",
                table: "stay_nights");

            migrationBuilder.DropIndex(
                name: "IX_stay_nights_StayId_Date",
                table: "stay_nights");

            migrationBuilder.DropIndex(
                name: "IX_stay_nights_RoomId_Date",
                table: "stay_nights");

            migrationBuilder.DropColumn("StayId", "stay_nights");
            migrationBuilder.DropColumn("RoomId", "stay_nights");
            migrationBuilder.DropColumn("DiscountPercent", "stay_nights");
            migrationBuilder.DropColumn("Status", "stay_nights");
            migrationBuilder.DropColumn("Description", "stay_nights");
            migrationBuilder.DropColumn("ClosedAt", "stay_nights");

            migrationBuilder.RenameColumn(
                name: "TariffAmount",
                table: "stay_nights",
                newName: "RoomPrice");

            migrationBuilder.DropTable("stays");
        }
    }
}
