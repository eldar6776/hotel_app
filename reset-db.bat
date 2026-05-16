@echo off
echo ========================================
echo   HotelPRO - Database Reset
echo ========================================
echo.
set PGPASSWORD=mihrivode
echo Dropping and recreating database...
"C:\Program Files\PostgreSQL\18\bin\psql.exe" -U postgres -c "DROP DATABASE IF EXISTS hotelpro;"
"C:\Program Files\PostgreSQL\18\bin\createdb.exe" -U postgres hotelpro
echo.
echo Running migrations...
set HOTEL_DB_CONN=Host=localhost;Port=5432;Database=hotelpro;Username=postgres;Password=mihrivode
set HOTEL_JWT_SECRET=hotelpro-dev-secret-mihrivode-2026
cd /d D:\ProjektiOtvoreni\hotel_app\backend\src\HotelPro.Infrastructure
dotnet ef database update --startup-project ..\HotelPro.Api\HotelPro.Api.csproj
echo.
echo Database reset complete with fresh data!
pause
