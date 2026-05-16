using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGuestGdprAndDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdDocumentNumber",
                table: "guests");

            migrationBuilder.DropColumn(
                name: "IdDocumentType",
                table: "guests");

            migrationBuilder.RenameColumn(
                name: "Nationality",
                table: "guests",
                newName: "GdprConsentVersion");

            migrationBuilder.AddColumn<DateTime>(
                name: "GdprConsentDate",
                table: "guests",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "GdprConsentGiven",
                table: "guests",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "NationalityCountryId",
                table: "guests",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DocumentType",
                table: "guest_documents",
                type: "integer",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "BackImagePath",
                table: "guest_documents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "guest_documents",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "FrontImagePath",
                table: "guest_documents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MRZLine1",
                table: "guest_documents",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MRZLine2",
                table: "guest_documents",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GdprConsentDate",
                table: "guests");

            migrationBuilder.DropColumn(
                name: "GdprConsentGiven",
                table: "guests");

            migrationBuilder.DropColumn(
                name: "NationalityCountryId",
                table: "guests");

            migrationBuilder.DropColumn(
                name: "BackImagePath",
                table: "guest_documents");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "guest_documents");

            migrationBuilder.DropColumn(
                name: "FrontImagePath",
                table: "guest_documents");

            migrationBuilder.DropColumn(
                name: "MRZLine1",
                table: "guest_documents");

            migrationBuilder.DropColumn(
                name: "MRZLine2",
                table: "guest_documents");

            migrationBuilder.RenameColumn(
                name: "GdprConsentVersion",
                table: "guests",
                newName: "Nationality");

            migrationBuilder.AddColumn<string>(
                name: "IdDocumentNumber",
                table: "guests",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IdDocumentType",
                table: "guests",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DocumentType",
                table: "guest_documents",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer",
                oldMaxLength: 20);
        }
    }
}
