using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingGroupAndMasterBill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_group_bookings_bookings_MemberBookingId",
                table: "group_bookings");

            migrationBuilder.DropColumn(
                name: "GroupName",
                table: "group_bookings");

            migrationBuilder.RenameColumn(
                name: "MemberBookingId",
                table: "group_bookings",
                newName: "RoomTypeId");

            migrationBuilder.RenameIndex(
                name: "IX_group_bookings_MemberBookingId",
                table: "group_bookings",
                newName: "IX_group_bookings_RoomTypeId");

            migrationBuilder.AddColumn<Guid>(
                name: "GroupId",
                table: "group_bookings",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "booking_groups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HotelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactPersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Arrival = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Departure = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    BlockedRoomCount = table.Column<int>(type: "integer", nullable: false),
                    ConfirmedRoomCount = table.Column<int>(type: "integer", nullable: false),
                    RatePlanId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UseMasterBill = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_groups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_booking_groups_guests_ContactPersonId",
                        column: x => x.ContactPersonId,
                        principalTable: "guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "master_bills",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayerGuestId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalStayCharges = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsClosed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_master_bills", x => x.Id);
                    table.ForeignKey(
                        name: "FK_master_bills_booking_groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "booking_groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_master_bills_guests_PayerGuestId",
                        column: x => x.PayerGuestId,
                        principalTable: "guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_group_bookings_GroupId_BookingId",
                table: "group_bookings",
                columns: new[] { "GroupId", "BookingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_booking_groups_Arrival",
                table: "booking_groups",
                column: "Arrival");

            migrationBuilder.CreateIndex(
                name: "IX_booking_groups_ContactPersonId",
                table: "booking_groups",
                column: "ContactPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_booking_groups_Departure",
                table: "booking_groups",
                column: "Departure");

            migrationBuilder.CreateIndex(
                name: "IX_booking_groups_HotelId",
                table: "booking_groups",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_booking_groups_ReleaseDate",
                table: "booking_groups",
                column: "ReleaseDate");

            migrationBuilder.CreateIndex(
                name: "IX_booking_groups_Status",
                table: "booking_groups",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_master_bills_GroupId",
                table: "master_bills",
                column: "GroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_master_bills_PayerGuestId",
                table: "master_bills",
                column: "PayerGuestId");

            migrationBuilder.AddForeignKey(
                name: "FK_bookings_booking_groups_GroupId",
                table: "bookings",
                column: "GroupId",
                principalTable: "booking_groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_group_bookings_booking_groups_GroupId",
                table: "group_bookings",
                column: "GroupId",
                principalTable: "booking_groups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_group_bookings_room_types_RoomTypeId",
                table: "group_bookings",
                column: "RoomTypeId",
                principalTable: "room_types",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_bookings_booking_groups_GroupId",
                table: "bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_group_bookings_booking_groups_GroupId",
                table: "group_bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_group_bookings_room_types_RoomTypeId",
                table: "group_bookings");

            migrationBuilder.DropTable(
                name: "master_bills");

            migrationBuilder.DropTable(
                name: "booking_groups");

            migrationBuilder.DropIndex(
                name: "IX_group_bookings_GroupId_BookingId",
                table: "group_bookings");

            migrationBuilder.DropColumn(
                name: "GroupId",
                table: "group_bookings");

            migrationBuilder.RenameColumn(
                name: "RoomTypeId",
                table: "group_bookings",
                newName: "MemberBookingId");

            migrationBuilder.RenameIndex(
                name: "IX_group_bookings_RoomTypeId",
                table: "group_bookings",
                newName: "IX_group_bookings_MemberBookingId");

            migrationBuilder.AddColumn<string>(
                name: "GroupName",
                table: "group_bookings",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_group_bookings_bookings_MemberBookingId",
                table: "group_bookings",
                column: "MemberBookingId",
                principalTable: "bookings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
