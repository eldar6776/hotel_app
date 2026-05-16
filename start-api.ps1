$env:HOTEL_DB_CONN="Host=localhost;Port=5432;Database=hotelpro;Username=postgres;Password=mihrivode"
$env:HOTEL_JWT_SECRET="hotelpro-dev-secret-mihrivode-2026"
$env:ASPNETCORE_URLS="http://localhost:5149"

Write-Host "Starting HotelPRO API..."
Set-Location D:\ProjektiOtvoreni\hotel_app\backend\src\HotelPro.Api
dotnet run --no-launch-profile
