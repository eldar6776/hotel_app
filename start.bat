@echo off
setlocal enabledelayedexpansion

REM ============================================================
REM   HotelPRO - Universal Startup Script
REM   Auto-detects project drive (C: or D: or any)
REM ============================================================

REM Get the drive where this script is located
set "PROJECT_DRIVE=%~d0"
set "PROJECT_ROOT=%~dp0"
set "PROJECT_ROOT=%PROJECT_ROOT:~0,-1%"

echo ========================================
echo   HotelPRO - Startup
echo ========================================
echo.
echo   Project location: %PROJECT_ROOT%
echo   Drive: %PROJECT_DRIVE%
echo.

REM Start Backend (port 5149)
echo   Starting Backend (port 5149)...
start "HotelPRO API" cmd /c "%PROJECT_ROOT%\start-api.bat"
echo.
echo   Waiting 15 seconds for backend...
timeout /t 15 /nobreak >nul
echo.

REM Start Frontend (port 3000)
echo   Starting Frontend (port 3000)...
start "HotelPRO Frontend" cmd /c "cd /d "%PROJECT_ROOT%\frontend" && npm run dev"
echo.
echo ========================================
echo   Backend:  http://localhost:5149/swagger
echo   Frontend: http://localhost:3000
echo.
echo   Login: admin@hotelpro.local / admin123
echo ========================================
pause
