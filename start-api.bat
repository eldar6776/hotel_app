@echo off
echo ========================================
echo   HotelPRO Backend API
echo ========================================
echo.
set HOTEL_DB_CONN=Host=localhost;Port=5432;Database=hotelpro;Username=postgres;Password=mihrivode
set HOTEL_JWT_SECRET=hotelpro-dev-secret-mihrivode-2026
set ASPNETCORE_URLS=http://localhost:5149
cd /d D:\ProjektiOtvoreni\hotel_app\backend\src\HotelPro.Api
echo Starting... wait for "Now listening on: http://localhost:5149"
echo.
dotnet run --no-build
pause
