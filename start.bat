@echo off
echo ========================================
echo   HotelPRO - Startup Script
echo ========================================
echo.

REM PostgreSQL 18 credentials
set HOTEL_DB_CONN=Host=localhost;Port=5432;Database=hotelpro;Username=postgres;Password=mihrivode
set HOTEL_JWT_SECRET=hotelpro-dev-secret-mihrivode-2026

REM Backend port
set ASPNETCORE_URLS=http://localhost:5149

echo [1/2] Starting Backend API (port 5149)...
start "HotelPRO Backend" cmd /c "cd /d D:\ProjektiOtvoreni\hotel_app\backend\src\HotelPro.Api && dotnet run"

echo Waiting for backend to start...
timeout /t 12 /nobreak >nul

echo.
echo [2/2] Starting Frontend (port 3000)...
start "HotelPRO Frontend" cmd /c "cd /d D:\ProjektiOtvoreni\hotel_app\frontend && npm run dev"

echo.
echo ========================================
echo   HotelPRO je pokrenut!
echo.
echo   Backend:  http://localhost:5149
echo   Swagger:  http://localhost:5149/swagger
echo   Frontend: http://localhost:3000
echo.
echo   Login: admin@hotelpro.local / admin123
echo ========================================
echo.
echo Zatvori prozore sa Ctrl+C ili X.
pause
