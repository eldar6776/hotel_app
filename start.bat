@echo off
echo ========================================
echo   HotelPRO - Startup
echo ========================================
echo.
echo Starting Backend (port 5149)...
start "HotelPRO API" cmd /c "C:\ProjektiOtvoreni\hotel_app\start-api.bat"
echo.
echo Waiting 15 seconds for backend...
timeout /t 15 /nobreak >nul
echo.
echo Starting Frontend (port 3000)...
start "HotelPRO Frontend" cmd /c "cd /d C:\ProjektiOtvoreni\hotel_app\frontend && npm run dev"
echo.
echo ========================================
echo   Backend:  http://localhost:5149/swagger
echo   Frontend: http://localhost:3000
echo.
echo   Login: admin@hotelpro.local / admin123
echo ========================================
pause
