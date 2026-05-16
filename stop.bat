@echo off
echo ========================================
echo   HotelPRO - STOP Script
echo ========================================
echo.
echo Killing all dotnet and node processes for HotelPRO...
taskkill /fi "WINDOWTITLE eq HotelPRO*" /f 2>nul
taskkill /im dotnet.exe /f 2>nul
taskkill /im node.exe /f 2>nul
echo.
echo Done. All processes stopped.
pause
