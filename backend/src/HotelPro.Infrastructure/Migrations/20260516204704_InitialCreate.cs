using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelPro.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdvancePayments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PaymentMethod = table.Column<string>(type: "text", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: true),
                    AppliedToInvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsRefunded = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvancePayments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "amenities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_amenities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityName = table.Column<string>(type: "text", nullable: false),
                    EntityId = table.Column<string>(type: "text", nullable: false),
                    Action = table.Column<string>(type: "text", nullable: false),
                    OldValues = table.Column<string>(type: "jsonb", nullable: true),
                    NewValues = table.Column<string>(type: "jsonb", nullable: true),
                    ChangedProperties = table.Column<string>(type: "jsonb", nullable: true),
                    ChangedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangedByEmail = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "text", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "buildings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_buildings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Nationality = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PhoneCode = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DayLocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LockedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LockedById = table.Column<Guid>(type: "uuid", nullable: true),
                    LockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UnlockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UnlockReason = table.Column<string>(type: "text", nullable: true),
                    UnlockedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayLocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Role = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PinHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CanLogin = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedBuildingIds = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeRates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrencyCode = table.Column<string>(type: "text", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric", nullable: false),
                    IsLocalCurrency = table.Column<bool>(type: "boolean", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Source = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "feature_flags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FeatureName = table.Column<string>(type: "text", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    RolloutPercentage = table.Column<int>(type: "integer", nullable: false),
                    HotelId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_feature_flags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "hotels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "EUR"),
                    TimeZone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Europe/Zagreb"),
                    VatNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hotels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceSequences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Prefix = table.Column<string>(type: "text", nullable: false),
                    LastNumber = table.Column<int>(type: "integer", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceSequences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "legacy_id_mapping",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityType = table.Column<string>(type: "text", nullable: false),
                    LegacyTableName = table.Column<string>(type: "text", nullable: false),
                    LegacyId = table.Column<int>(type: "integer", nullable: false),
                    NewId = table.Column<Guid>(type: "uuid", nullable: false),
                    MigratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_legacy_id_mapping", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "night_audit_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RanAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BookingsProcessed = table.Column<int>(type: "integer", nullable: false),
                    TotalStayCharges = table.Column<decimal>(type: "numeric", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_night_audit_logs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "payment_methods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_methods", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProformaInvoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProformaNumber = table.Column<string>(type: "text", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ConvertedToInvoiceId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProformaInvoices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "room_types",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    BaseCapacity = table.Column<int>(type: "integer", nullable: false),
                    MaxCapacity = table.Column<int>(type: "integer", nullable: false),
                    DefaultPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_types", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "service_catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    DefaultPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    VatPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_service_catalog", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "guests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    PostalCode = table.Column<string>(type: "text", nullable: true),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: true),
                    NationalityCountryId = table.Column<int>(type: "integer", nullable: true),
                    IsCompany = table.Column<bool>(type: "boolean", nullable: false),
                    CompanyName = table.Column<string>(type: "text", nullable: true),
                    VatNumber = table.Column<string>(type: "text", nullable: true),
                    GdprConsentGiven = table.Column<bool>(type: "boolean", nullable: false),
                    GdprConsentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    GdprConsentVersion = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_guests_countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "partners",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    PartnerType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    ContactPerson = table.Column<string>(type: "text", nullable: true),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    City = table.Column<string>(type: "text", nullable: true),
                    CountryId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContractCode = table.Column<string>(type: "text", nullable: true),
                    CommissionPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_partners", x => x.Id);
                    table.ForeignKey(
                        name: "FK_partners_countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "access_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: false),
                    UserAgent = table.Column<string>(type: "text", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_access_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_access_logs_employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "expenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    VatAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ExpenseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SupplierName = table.Column<string>(type: "text", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "text", nullable: true),
                    ApprovedById = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_expenses_employees_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "refresh_tokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    ReplacedByToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_refresh_tokens_employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    ShiftType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_shifts_employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomNumber = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Floor = table.Column<int>(type: "integer", nullable: false),
                    BuildingId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    BasePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_rooms_buildings_BuildingId",
                        column: x => x.BuildingId,
                        principalTable: "buildings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_rooms_room_types_RoomTypeId",
                        column: x => x.RoomTypeId,
                        principalTable: "room_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tariffs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RoomTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    BasePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tariffs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tariffs_room_types_RoomTypeId",
                        column: x => x.RoomTypeId,
                        principalTable: "room_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
                name: "guest_documents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuestId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentType = table.Column<int>(type: "integer", maxLength: 20, nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IssuingCountry = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MRZLine1 = table.Column<string>(type: "text", nullable: true),
                    MRZLine2 = table.Column<string>(type: "text", nullable: true),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FileUrl = table.Column<string>(type: "text", nullable: true),
                    FrontImagePath = table.Column<string>(type: "text", nullable: true),
                    BackImagePath = table.Column<string>(type: "text", nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_guest_documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_guest_documents_guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sales_agents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "text", nullable: true),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    PartnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CommissionPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sales_agents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sales_agents_partners_PartnerId",
                        column: x => x.PartnerId,
                        principalTable: "partners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "housekeeping_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false),
                    VerifiedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_housekeeping_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_housekeeping_logs_employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_housekeeping_logs_employees_VerifiedById",
                        column: x => x.VerifiedById,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_housekeeping_logs_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "phone_extensions",
                columns: table => new
                {
                    Extension = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_phone_extensions", x => x.Extension);
                    table.ForeignKey(
                        name: "FK_phone_extensions_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "room_assignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuestId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArrivalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DepartureDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_assignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_room_assignments_guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_room_assignments_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "room_out_of_order",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    ResolvedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_room_out_of_order", x => x.Id);
                    table.ForeignKey(
                        name: "FK_room_out_of_order_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "work_orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReportedById = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedToId = table.Column<Guid>(type: "uuid", nullable: true),
                    Priority = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Category = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_work_orders_employees_AssignedToId",
                        column: x => x.AssignedToId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_work_orders_employees_ReportedById",
                        column: x => x.ReportedById,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_work_orders_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    HotelId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuestId = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ArrivalDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DepartureDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AdultCount = table.Column<int>(type: "integer", nullable: false),
                    ChildCount = table.Column<int>(type: "integer", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ExchangeRateTotal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    InternalNotes = table.Column<string>(type: "text", nullable: true),
                    CancellationReason = table.Column<string>(type: "text", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bookings_booking_groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "booking_groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_bookings_guests_GuestId",
                        column: x => x.GuestId,
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

            migrationBuilder.CreateTable(
                name: "booking_histories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PreviousValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    ChangedById = table.Column<Guid>(type: "uuid", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_histories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_booking_histories_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_booking_histories_employees_ChangedById",
                        column: x => x.ChangedById,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "booking_rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    RoomTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RatePlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    PricePerNight = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_booking_rooms_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_booking_rooms_room_types_RoomTypeId",
                        column: x => x.RoomTypeId,
                        principalTable: "room_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_booking_rooms_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "email_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    Recipient = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Subject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    IsHtml = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    RetryCount = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_email_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_email_logs_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "group_bookings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_group_bookings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_group_bookings_booking_groups_GroupId",
                        column: x => x.GroupId,
                        principalTable: "booking_groups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_group_bookings_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_group_bookings_room_types_RoomTypeId",
                        column: x => x.RoomTypeId,
                        principalTable: "room_types",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GuestStayHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuestId = table.Column<Guid>(type: "uuid", nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckedInAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckedOutAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RoomNumber = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuestStayHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuestStayHistories_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuestStayHistories_guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuestStayHistories_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "folios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FolioNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    BookingId = table.Column<Guid>(type: "uuid", nullable: true),
                    BookingRoomId = table.Column<Guid>(type: "uuid", nullable: true),
                    GuestId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_folios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_folios_booking_rooms_BookingRoomId",
                        column: x => x.BookingRoomId,
                        principalTable: "booking_rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_folios_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_folios_guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "charges",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FolioId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceCatalogId = table.Column<Guid>(type: "uuid", nullable: true),
                    ChargeType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    VatAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    ChargeDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PostedById = table.Column<Guid>(type: "uuid", nullable: true),
                    IsTaxable = table.Column<bool>(type: "boolean", nullable: false),
                    POSReference = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_charges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_charges_employees_PostedById",
                        column: x => x.PostedById,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_charges_folios_FolioId",
                        column: x => x.FolioId,
                        principalTable: "folios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_charges_service_catalog_ServiceCatalogId",
                        column: x => x.ServiceCatalogId,
                        principalTable: "service_catalog",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FolioId = table.Column<Guid>(type: "uuid", nullable: false),
                    GuestId = table.Column<Guid>(type: "uuid", nullable: true),
                    IssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TotalNet = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalVat = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    TotalGross = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PdfUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_invoices_folios_FolioId",
                        column: x => x.FolioId,
                        principalTable: "folios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_invoices_guests_GuestId",
                        column: x => x.GuestId,
                        principalTable: "guests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "outstanding_balances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FolioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsOverdue = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outstanding_balances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_outstanding_balances_folios_FolioId",
                        column: x => x.FolioId,
                        principalTable: "folios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FolioId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentMethodId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reference = table.Column<string>(type: "text", nullable: true),
                    ProcessedById = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_payments_employees_ProcessedById",
                        column: x => x.ProcessedById,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_payments_folios_FolioId",
                        column: x => x.FolioId,
                        principalTable: "folios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payments_payment_methods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "payment_methods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stay_nights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FolioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RoomPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    IsComp = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stay_nights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_stay_nights_folios_FolioId",
                        column: x => x.FolioId,
                        principalTable: "folios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "invoice_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    VatAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    GrossAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    VatPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoice_items", x => x.Id);
                    table.ForeignKey(
                        name: "FK_invoice_items_invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_access_logs_EmployeeId",
                table: "access_logs",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_access_logs_Timestamp",
                table: "access_logs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_ChangedAt",
                table: "audit_logs",
                column: "ChangedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_EntityName_EntityId",
                table: "audit_logs",
                columns: new[] { "EntityName", "EntityId" });

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
                name: "IX_booking_histories_BookingId",
                table: "booking_histories",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_booking_histories_ChangedAt",
                table: "booking_histories",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_booking_histories_ChangedById",
                table: "booking_histories",
                column: "ChangedById");

            migrationBuilder.CreateIndex(
                name: "IX_booking_rooms_BookingId",
                table: "booking_rooms",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_booking_rooms_RoomId",
                table: "booking_rooms",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_booking_rooms_RoomTypeId",
                table: "booking_rooms",
                column: "RoomTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_ArrivalDate",
                table: "bookings",
                column: "ArrivalDate");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_DepartureDate",
                table: "bookings",
                column: "DepartureDate");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_GroupId",
                table: "bookings",
                column: "GroupId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_GuestId",
                table: "bookings",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_HotelId",
                table: "bookings",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_Status",
                table: "bookings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_charges_FolioId",
                table: "charges",
                column: "FolioId");

            migrationBuilder.CreateIndex(
                name: "IX_charges_PostedById",
                table: "charges",
                column: "PostedById");

            migrationBuilder.CreateIndex(
                name: "IX_charges_ServiceCatalogId",
                table: "charges",
                column: "ServiceCatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_email_logs_BookingId",
                table: "email_logs",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_email_logs_CreatedAt",
                table: "email_logs",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_email_logs_Status",
                table: "email_logs",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_employees_Email",
                table: "employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_expenses_ApprovedById",
                table: "expenses",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_feature_flags_FeatureName_HotelId",
                table: "feature_flags",
                columns: new[] { "FeatureName", "HotelId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_folios_BookingId",
                table: "folios",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_folios_BookingRoomId",
                table: "folios",
                column: "BookingRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_folios_GuestId",
                table: "folios",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_group_bookings_BookingId",
                table: "group_bookings",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_group_bookings_GroupId_BookingId",
                table: "group_bookings",
                columns: new[] { "GroupId", "BookingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_group_bookings_RoomTypeId",
                table: "group_bookings",
                column: "RoomTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_guest_documents_GuestId",
                table: "guest_documents",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_guests_CountryId",
                table: "guests",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_guests_Email",
                table: "guests",
                column: "Email",
                unique: true,
                filter: "\"Email\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_guests_LastName_FirstName",
                table: "guests",
                columns: new[] { "LastName", "FirstName" });

            migrationBuilder.CreateIndex(
                name: "IX_GuestStayHistories_BookingId",
                table: "GuestStayHistories",
                column: "BookingId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestStayHistories_GuestId",
                table: "GuestStayHistories",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_GuestStayHistories_RoomId",
                table: "GuestStayHistories",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_hotels_Code",
                table: "hotels",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_housekeeping_logs_EmployeeId",
                table: "housekeeping_logs",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_housekeeping_logs_RoomId_Status",
                table: "housekeeping_logs",
                columns: new[] { "RoomId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_housekeeping_logs_VerifiedById",
                table: "housekeeping_logs",
                column: "VerifiedById");

            migrationBuilder.CreateIndex(
                name: "IX_invoice_items_InvoiceId",
                table: "invoice_items",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_FolioId",
                table: "invoices",
                column: "FolioId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_GuestId",
                table: "invoices",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_InvoiceNumber",
                table: "invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_legacy_id_mapping_EntityType_LegacyId",
                table: "legacy_id_mapping",
                columns: new[] { "EntityType", "LegacyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_master_bills_GroupId",
                table: "master_bills",
                column: "GroupId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_master_bills_PayerGuestId",
                table: "master_bills",
                column: "PayerGuestId");

            migrationBuilder.CreateIndex(
                name: "IX_night_audit_logs_AuditDate",
                table: "night_audit_logs",
                column: "AuditDate",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_outstanding_balances_FolioId",
                table: "outstanding_balances",
                column: "FolioId");

            migrationBuilder.CreateIndex(
                name: "IX_partners_CountryId",
                table: "partners",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_partners_Name",
                table: "partners",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_payments_FolioId",
                table: "payments",
                column: "FolioId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_PaymentMethodId",
                table: "payments",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_payments_ProcessedById",
                table: "payments",
                column: "ProcessedById");

            migrationBuilder.CreateIndex(
                name: "IX_phone_extensions_RoomId",
                table: "phone_extensions",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_EmployeeId",
                table: "refresh_tokens",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_refresh_tokens_Token",
                table: "refresh_tokens",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_room_assignments_GuestId",
                table: "room_assignments",
                column: "GuestId");

            migrationBuilder.CreateIndex(
                name: "IX_room_assignments_RoomId",
                table: "room_assignments",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_room_out_of_order_RoomId",
                table: "room_out_of_order",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_room_out_of_order_RoomId_Status",
                table: "room_out_of_order",
                columns: new[] { "RoomId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_rooms_BuildingId",
                table: "rooms",
                column: "BuildingId");

            migrationBuilder.CreateIndex(
                name: "IX_rooms_RoomNumber_BuildingId",
                table: "rooms",
                columns: new[] { "RoomNumber", "BuildingId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rooms_RoomTypeId",
                table: "rooms",
                column: "RoomTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_sales_agents_PartnerId",
                table: "sales_agents",
                column: "PartnerId");

            migrationBuilder.CreateIndex(
                name: "IX_shifts_EmployeeId_ShiftDate",
                table: "shifts",
                columns: new[] { "EmployeeId", "ShiftDate" });

            migrationBuilder.CreateIndex(
                name: "IX_stay_nights_FolioId",
                table: "stay_nights",
                column: "FolioId");

            migrationBuilder.CreateIndex(
                name: "IX_tariffs_RoomTypeId",
                table: "tariffs",
                column: "RoomTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_AssignedToId",
                table: "work_orders",
                column: "AssignedToId");

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_ReportedById",
                table: "work_orders",
                column: "ReportedById");

            migrationBuilder.CreateIndex(
                name: "IX_work_orders_RoomId",
                table: "work_orders",
                column: "RoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "access_logs");

            migrationBuilder.DropTable(
                name: "AdvancePayments");

            migrationBuilder.DropTable(
                name: "amenities");

            migrationBuilder.DropTable(
                name: "audit_logs");

            migrationBuilder.DropTable(
                name: "booking_histories");

            migrationBuilder.DropTable(
                name: "charges");

            migrationBuilder.DropTable(
                name: "DayLocks");

            migrationBuilder.DropTable(
                name: "email_logs");

            migrationBuilder.DropTable(
                name: "ExchangeRates");

            migrationBuilder.DropTable(
                name: "expenses");

            migrationBuilder.DropTable(
                name: "feature_flags");

            migrationBuilder.DropTable(
                name: "group_bookings");

            migrationBuilder.DropTable(
                name: "guest_documents");

            migrationBuilder.DropTable(
                name: "GuestStayHistories");

            migrationBuilder.DropTable(
                name: "hotels");

            migrationBuilder.DropTable(
                name: "housekeeping_logs");

            migrationBuilder.DropTable(
                name: "invoice_items");

            migrationBuilder.DropTable(
                name: "InvoiceSequences");

            migrationBuilder.DropTable(
                name: "legacy_id_mapping");

            migrationBuilder.DropTable(
                name: "master_bills");

            migrationBuilder.DropTable(
                name: "night_audit_logs");

            migrationBuilder.DropTable(
                name: "outstanding_balances");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "phone_extensions");

            migrationBuilder.DropTable(
                name: "ProformaInvoices");

            migrationBuilder.DropTable(
                name: "refresh_tokens");

            migrationBuilder.DropTable(
                name: "room_assignments");

            migrationBuilder.DropTable(
                name: "room_out_of_order");

            migrationBuilder.DropTable(
                name: "sales_agents");

            migrationBuilder.DropTable(
                name: "shifts");

            migrationBuilder.DropTable(
                name: "stay_nights");

            migrationBuilder.DropTable(
                name: "tariffs");

            migrationBuilder.DropTable(
                name: "work_orders");

            migrationBuilder.DropTable(
                name: "service_catalog");

            migrationBuilder.DropTable(
                name: "invoices");

            migrationBuilder.DropTable(
                name: "payment_methods");

            migrationBuilder.DropTable(
                name: "partners");

            migrationBuilder.DropTable(
                name: "employees");

            migrationBuilder.DropTable(
                name: "folios");

            migrationBuilder.DropTable(
                name: "booking_rooms");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "booking_groups");

            migrationBuilder.DropTable(
                name: "buildings");

            migrationBuilder.DropTable(
                name: "room_types");

            migrationBuilder.DropTable(
                name: "guests");

            migrationBuilder.DropTable(
                name: "countries");
        }
    }
}
