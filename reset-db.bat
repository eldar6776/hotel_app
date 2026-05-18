@echo off
setlocal enabledelayedexpansion

REM ============================================================
REM   HotelPRO - Database Reset (Universal - auto-detects drive)
REM ============================================================

set "PROJECT_ROOT=%~dp0"
set "PROJECT_ROOT=%PROJECT_ROOT:~0,-1%"

echo ========================================
echo   HotelPRO - Database Reset
echo ========================================
echo.
echo   Project root: %PROJECT_ROOT%
echo.

set PGPASSWORD=mihrivode

REM Try common PostgreSQL install locations
if exist "C:\Program Files\PostgreSQL\18\bin\psql.exe" (
    set "PG_BIN=C:\Program Files\PostgreSQL\18\bin"
) else if exist "D:\Program Files\PostgreSQL\18\bin\psql.exe" (
    set "PG_BIN=D:\Program Files\PostgreSQL\18\bin"
) else if exist "C:\Program Files\PostgreSQL\17\bin\psql.exe" (
    set "PG_BIN=C:\Program Files\PostgreSQL\17\bin"
) else if exist "D:\Program Files\PostgreSQL\17\bin\psql.exe" (
    set "PG_BIN=D:\Program Files\PostgreSQL\17\bin"
) else (
    echo WARNING: PostgreSQL not found in standard locations.
    echo Searching PATH for psql...
    where psql >nul 2>&1
    if %errorlevel% equ 0 (
        set "PG_BIN="
    ) else (
        echo ERROR: psql not found. Please install PostgreSQL or add to PATH.
        pause
        exit /b 1
    )
)

echo Dropping and recreating database...
if defined PG_BIN (
    "%PG_BIN%\psql.exe" -U postgres -c "DROP DATABASE IF EXISTS hotelpro;"
    "%PG_BIN%\createdb.exe" -U postgres hotelpro
) else (
    psql -U postgres -c "DROP DATABASE IF EXISTS hotelpro;"
    createdb -U postgres hotelpro
)
echo.

echo Running migrations...
set HOTEL_DB_CONN=Host=localhost;Port=5432;Database=hotelpro;Username=postgres;Password=mihrivode
set HOTEL_JWT_SECRET=hotelpro-dev-secret-mihrivode-2026
cd /d "%PROJECT_ROOT%\backend\src\HotelPro.Infrastructure"
dotnet ef database update --startup-project ..\HotelPro.Api\HotelPro.Api.csproj
echo.
echo Database reset complete with fresh data!
pause
