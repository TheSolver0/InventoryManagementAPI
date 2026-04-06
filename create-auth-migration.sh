#!/bin/bash
# Create and apply migrations for authentication

echo "Creating migration for User table..."
dotnet ef migrations add AddUserTable --project GestionDeStock.API/GestionDeStock.API.csproj

echo "Applying migrations..."
dotnet ef database update --project GestionDeStock.API/GestionDeStock.API.csproj

echo "✅ Migration completed successfully!"
echo "You can now register and login using the /api/auth endpoints."
