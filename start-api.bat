@echo off
setlocal enabledelayedexpansion

REM ============================================================
REM   HotelPRO Backend API - Universal (auto-detects drive)
REM ============================================================

set "PROJECT_ROOT=%~dp0"
set "PROJECT_ROOT=%PROJECT_ROOT:~0,-1%"

echo ========================================
echo   HotelPRO Backend API
echo ========================================
echo.
echo   Project root: %PROJECT_ROOT%
echo.

set HOTEL_DB_CONN=Host=localhost;Port=5432;Database=hotelpro;Username=postgres;Password=mihrivode
set HOTEL_JWT_SECRET=hotelpro-dev-secret-mihrivode-2026
set ASPNETCORE_URLS=http://localhost:5149

cd /d "%PROJECT_ROOT%\backend\src\HotelPro.Api"

echo   Starting... wait for "Now listening on: http://localhost:5149"
echo.
dotnet run --no-build
pause
