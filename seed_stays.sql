-- Stays already inserted. Only inserting stay_nights now.

-- Create stay_nights for Stay 1 (Katarina Pavic, Room 301, 2 nights: May 17-18, tariff 150/night)
INSERT INTO stay_nights ("Id", "FolioId", "Date", "TariffAmount", "IsComp", "StayId", "RoomId", "DiscountPercent", "Status", "Description")
VALUES
    (gen_random_uuid(), 'caf4fd43-603a-478e-9414-2980f9e85fe7', '2026-05-17 00:00:00+02', 150.00, false, 'a1111111-1111-1111-1111-111111111111', '3a263881-f5ec-4e67-a18c-58b6314a03f8', 0.00, 0, 'Night 1 - Room 301'),
    (gen_random_uuid(), 'caf4fd43-603a-478e-9414-2980f9e85fe7', '2026-05-18 00:00:00+02', 150.00, false, 'a1111111-1111-1111-1111-111111111111', '3a263881-f5ec-4e67-a18c-58b6314a03f8', 0.00, 0, 'Night 2 - Room 301');

-- Create stay_nights for Stay 2 (Ivan Vukovic, Room B202, 3 nights: May 17-19, tariff 80/night)
INSERT INTO stay_nights ("Id", "FolioId", "Date", "TariffAmount", "IsComp", "StayId", "RoomId", "DiscountPercent", "Status", "Description")
VALUES
    (gen_random_uuid(), '79b0f3b3-9c1b-4d0a-ab6c-d945736a2370', '2026-05-17 00:00:00+02', 80.00, false, 'a2222222-2222-2222-2222-222222222222', '3afcc319-fe76-4236-89e6-75a6597ac6f9', 0.00, 0, 'Night 1 - Room B202'),
    (gen_random_uuid(), '79b0f3b3-9c1b-4d0a-ab6c-d945736a2370', '2026-05-18 00:00:00+02', 80.00, false, 'a2222222-2222-2222-2222-222222222222', '3afcc319-fe76-4236-89e6-75a6597ac6f9', 0.00, 0, 'Night 2 - Room B202'),
    (gen_random_uuid(), '79b0f3b3-9c1b-4d0a-ab6c-d945736a2370', '2026-05-19 00:00:00+02', 80.00, false, 'a2222222-2222-2222-2222-222222222222', '3afcc319-fe76-4236-89e6-75a6597ac6f9', 0.00, 0, 'Night 3 - Room B202');

-- Create stay_nights for Stay 3 (Petar Babic, Room B104, 4 nights: May 17-20, tariff 50/night)
INSERT INTO stay_nights ("Id", "FolioId", "Date", "TariffAmount", "IsComp", "StayId", "RoomId", "DiscountPercent", "Status", "Description")
VALUES
    (gen_random_uuid(), '206a55f9-01d4-4c95-958e-f987f4c39d2f', '2026-05-17 00:00:00+02', 50.00, false, 'a3333333-3333-3333-3333-333333333333', '2df41463-f725-450e-8824-90cfcab548f1', 0.00, 0, 'Night 1 - Room B104'),
    (gen_random_uuid(), '206a55f9-01d4-4c95-958e-f987f4c39d2f', '2026-05-18 00:00:00+02', 50.00, false, 'a3333333-3333-3333-3333-333333333333', '2df41463-f725-450e-8824-90cfcab548f1', 0.00, 0, 'Night 2 - Room B104'),
    (gen_random_uuid(), '206a55f9-01d4-4c95-958e-f987f4c39d2f', '2026-05-19 00:00:00+02', 50.00, false, 'a3333333-3333-3333-3333-333333333333', '2df41463-f725-450e-8824-90cfcab548f1', 0.00, 0, 'Night 3 - Room B104'),
    (gen_random_uuid(), '206a55f9-01d4-4c95-958e-f987f4c39d2f', '2026-05-20 00:00:00+02', 50.00, false, 'a3333333-3333-3333-3333-333333333333', '2df41463-f725-450e-8824-90cfcab548f1', 0.00, 0, 'Night 4 - Room B104');

-- Verify
SELECT 'stays' as tbl, count(*) FROM stays
UNION ALL
SELECT 'stay_nights', count(*) FROM stay_nights;
