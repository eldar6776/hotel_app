-- Seed payments for existing folios
-- Folio 1: Katarina Pavic, balance 300 EUR
INSERT INTO payments ("Id", "FolioId", "PaymentMethod", "Amount", "Status", "PaymentDate", "Reference", "Notes")
VALUES
    (gen_random_uuid(), 'caf4fd43-603a-478e-9414-2980f9e85fe7', 'Cash', 150.00, 'Completed', '2026-05-17 15:00:00+02', 'CASH-001', 'Advance payment on check-in');

-- Folio 2: Ivan Vukovic, balance 240 EUR
INSERT INTO payments ("Id", "FolioId", "PaymentMethod", "Amount", "Status", "PaymentDate", "Reference", "Notes")
VALUES
    (gen_random_uuid(), '79b0f3b3-9c1b-4d0a-ab6c-d945736a2370', 'Card', 120.00, 'Completed', '2026-05-17 15:30:00+02', 'CARD-001', 'Partial payment on check-in');

-- Folio 3: Petar Babic, balance 200 EUR
INSERT INTO payments ("Id", "FolioId", "PaymentMethod", "Amount", "Status", "PaymentDate", "Reference", "Notes")
VALUES
    (gen_random_uuid(), '206a55f9-01d4-4c95-958e-f987f4c39d2f', 'Cash', 100.00, 'Completed', '2026-05-17 16:00:00+02', 'CASH-002', 'Advance payment on check-in');

-- Verify
SELECT 'payments' as tbl, count(*) FROM payments;
