-- Fix hotel code for localhost tenant resolution
UPDATE hotels SET "Code" = 'localhost' WHERE "Id" = '5337c012-480b-4605-ae54-04ec20cf2eef';

-- Fix bookings HotelId to point to real hotel
UPDATE bookings SET "HotelId" = '5337c012-480b-4605-ae54-04ec20cf2eef' WHERE "HotelId" = '00000000-0000-0000-0000-000000000000';

-- Verify
SELECT "Id", "Name", "Code" FROM hotels;
SELECT count(*) as fixed_bookings FROM bookings WHERE "HotelId" = '5337c012-480b-4605-ae54-04ec20cf2eef';
