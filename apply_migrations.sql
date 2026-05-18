-- Migration: AddHotelConfig
CREATE TABLE IF NOT EXISTS hotel_configs (
    "Id" uuid NOT NULL,
    "HotelId" uuid NOT NULL,
    "Key" varchar(100) NOT NULL,
    "Value" text NOT NULL,
    "Category" varchar(50) NOT NULL,
    "Description" varchar(500) NULL,
    "IsSecret" boolean NOT NULL DEFAULT false,
    "IsEnabled" boolean NOT NULL DEFAULT false,
    "CreatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
    "UpdatedAt" timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT PK_hotel_configs PRIMARY KEY ("Id")
);
CREATE INDEX IF NOT EXISTS IX_hotel_configs_HotelId_Category ON hotel_configs ("HotelId", "Category");
CREATE INDEX IF NOT EXISTS IX_hotel_configs_HotelId_Key ON hotel_configs ("HotelId", "Key");

-- Migration: AddStayAndExtendStayNight
CREATE TABLE IF NOT EXISTS stays (
    "Id" uuid NOT NULL,
    "HotelId" uuid NOT NULL,
    "GuestId" uuid NOT NULL,
    "RoomId" uuid NOT NULL,
    "FolioId" uuid NULL,
    "BookingId" uuid NULL,
    "BookingRoomId" uuid NULL,
    "TariffId" uuid NULL,
    "CheckInDate" timestamp with time zone NOT NULL,
    "CheckOutDate" timestamp with time zone NOT NULL,
    "CheckedInBy" uuid NULL,
    "CheckedOutBy" uuid NULL,
    "CheckedOutAt" timestamp with time zone NULL,
    "IsCheckedOut" boolean NOT NULL DEFAULT false,
    "IsRegistrationPrinted" boolean NOT NULL DEFAULT false,
    "IsReservationLink" boolean NOT NULL DEFAULT false,
    "IsFromConfirmedReservation" boolean NOT NULL DEFAULT false,
    "IsAccommodationPaid" boolean NOT NULL DEFAULT false,
    "GuestCategory" integer NOT NULL DEFAULT 0,
    "DiscountPercent" decimal(5,2) NOT NULL DEFAULT 0,
    "DiscountReason" varchar(200) NULL,
    "TaxOverride" integer NOT NULL DEFAULT 0,
    "StayNote" varchar(1000) NULL,
    "ServiceNote" varchar(1000) NULL,
    "PaymentNote" varchar(1000) NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone NULL,
    CONSTRAINT PK_stays PRIMARY KEY ("Id")
);

CREATE INDEX IF NOT EXISTS IX_stays_RoomId_IsCheckedOut ON stays ("RoomId", "IsCheckedOut");
CREATE INDEX IF NOT EXISTS IX_stays_GuestId_IsCheckedOut ON stays ("GuestId", "IsCheckedOut");
CREATE INDEX IF NOT EXISTS IX_stays_FolioId ON stays ("FolioId");

ALTER TABLE stays ADD CONSTRAINT FK_stays_guests_GuestId FOREIGN KEY ("GuestId") REFERENCES guests("Id") ON DELETE RESTRICT;
ALTER TABLE stays ADD CONSTRAINT FK_stays_rooms_RoomId FOREIGN KEY ("RoomId") REFERENCES rooms("Id") ON DELETE RESTRICT;
ALTER TABLE stays ADD CONSTRAINT FK_stays_folios_FolioId FOREIGN KEY ("FolioId") REFERENCES folios("Id") ON DELETE RESTRICT;
ALTER TABLE stays ADD CONSTRAINT FK_stays_bookings_BookingId FOREIGN KEY ("BookingId") REFERENCES bookings("Id") ON DELETE SET NULL;
ALTER TABLE stays ADD CONSTRAINT FK_stays_booking_rooms_BookingRoomId FOREIGN KEY ("BookingRoomId") REFERENCES booking_rooms("Id") ON DELETE SET NULL;

-- Extend stay_nights
DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='stay_nights' AND column_name='StayId') THEN
        ALTER TABLE stay_nights ADD COLUMN "StayId" uuid NULL;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='stay_nights' AND column_name='RoomId') THEN
        ALTER TABLE stay_nights ADD COLUMN "RoomId" uuid NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='stay_nights' AND column_name='DiscountPercent') THEN
        ALTER TABLE stay_nights ADD COLUMN "DiscountPercent" decimal(5,2) NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='stay_nights' AND column_name='Status') THEN
        ALTER TABLE stay_nights ADD COLUMN "Status" integer NOT NULL DEFAULT 0;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='stay_nights' AND column_name='Description') THEN
        ALTER TABLE stay_nights ADD COLUMN "Description" varchar(500) NULL;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='stay_nights' AND column_name='ClosedAt') THEN
        ALTER TABLE stay_nights ADD COLUMN "ClosedAt" timestamp with time zone NULL;
    END IF;
    IF EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='stay_nights' AND column_name='RoomPrice') THEN
        ALTER TABLE stay_nights RENAME COLUMN "RoomPrice" TO "TariffAmount";
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS IX_stay_nights_StayId_Date ON stay_nights ("StayId", "Date");
CREATE INDEX IF NOT EXISTS IX_stay_nights_RoomId_Date ON stay_nights ("RoomId", "Date");

DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname='FK_stay_nights_stays_StayId') THEN
        ALTER TABLE stay_nights ADD CONSTRAINT FK_stay_nights_stays_StayId FOREIGN KEY ("StayId") REFERENCES stays("Id") ON DELETE SET NULL;
    END IF;
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname='FK_stay_nights_rooms_RoomId') THEN
        ALTER TABLE stay_nights ADD CONSTRAINT FK_stay_nights_rooms_RoomId FOREIGN KEY ("RoomId") REFERENCES rooms("Id") ON DELETE RESTRICT;
    END IF;
END $$;

-- Migration: AddStayIdToGuestStayHistory
DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.columns WHERE table_name='GuestStayHistories' AND column_name='StayId') THEN
        ALTER TABLE "GuestStayHistories" ADD COLUMN "StayId" uuid NULL;
    END IF;
END $$;

DO $$ BEGIN
    IF NOT EXISTS (SELECT 1 FROM pg_constraint WHERE conname='FK_GuestStayHistories_stays_StayId') THEN
        ALTER TABLE "GuestStayHistories" ADD CONSTRAINT FK_GuestStayHistories_stays_StayId FOREIGN KEY ("StayId") REFERENCES stays("Id") ON DELETE SET NULL;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS IX_GuestStayHistories_StayId ON "GuestStayHistories" ("StayId");

-- Register all migrations in history
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20260518095144_AddHotelConfig', '8.0.11');
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20260518220000_AddStayAndExtendStayNight', '8.0.11');
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion") VALUES ('20260518220100_AddStayIdToGuestStayHistory', '8.0.11');
