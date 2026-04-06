@echo off
REM Create and apply migrations for authentication

echo Creating migration for User table...
dotnet ef migrations add AddUserTable --project GestionDeStock.API/GestionDeStock.API.csproj

if errorlevel 1 (
    echo Error creating migration
    pause
    exit /b 1
)

echo Applying migrations...
dotnet ef database update --project GestionDeStock.API/GestionDeStock.API.csproj

if errorlevel 1 (
    echo Error applying migrations
    pause
    exit /b 1
)

echo.
echo ========================================
echo Migration completed successfully!
echo ========================================
echo.
echo You can now register and login using the /api/auth endpoints.
echo.
pause
