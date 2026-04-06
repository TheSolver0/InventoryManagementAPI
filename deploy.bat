@echo off
REM Inventory Management API - Deployment Script for Windows

echo.
echo Inventory Management API - Deployment
echo.

REM Check if Docker is installed
docker --version > nul 2>&1
if errorlevel 1 (
    echo Docker is not installed or not in PATH
    echo Please install Docker Desktop from https://www.docker.com/products/docker-desktop
    pause
    exit /b 1
)

REM Check if Docker Compose is installed
docker-compose --version > nul 2>&1
if errorlevel 1 (
    echo Docker Compose is not installed
    echo It should be included with Docker Desktop
    pause
    exit /b 1
)

REM Create .env file if it doesn't exist
if not exist .env (
    echo Creating .env file from .env.example...
    copy .env.example .env
    echo.
    echo WARNING: Please edit .env file with your configuration before continuing
    pause
    exit /b 1
)

REM Create uploads directory
if not exist uploads (
    mkdir uploads
)

echo.
echo Building Docker images...
docker-compose build

echo.
echo Starting services...
docker-compose up -d

echo.
echo Waiting for MySQL to be ready (30 seconds)...
timeout /t 30 /nobreak

echo.
echo Checking service status...
docker-compose ps

echo.
echo ======================================
echo Deployment completed successfully!
echo ======================================
echo.
echo API Information:
echo   - API URL: http://localhost:8080
echo   - Swagger UI: http://localhost:8080/swagger
echo   - MySQL Port: 3306
echo.
echo Useful commands:
echo   - View logs: docker-compose logs -f api
echo   - Stop services: docker-compose down
echo   - Restart services: docker-compose restart
echo.
pause
