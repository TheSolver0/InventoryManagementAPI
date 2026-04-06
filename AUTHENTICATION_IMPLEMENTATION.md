# Résumé des changements - Authentification JWT

## 📦 Packages NuGet ajoutés

```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.9" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.9.0" />
```

## 📁 Fichiers créés

### Code source
- **`Models/User.cs`** - Modèle de l'utilisateur
- **`Dtos/AuthDto.cs`** - DTOs pour l'authentification (RegisterDto, LoginDto, AuthResponseDto, UserDto)
- **`Services/AuthService.cs`** - Service d'authentification (login, register, JWT generation)
- **`Controllers/AuthController.cs`** - Contrôleur pour les routes /api/auth/login et /api/auth/register

### Configuration
- **`appsettings.json`** - Ajout des paramètres JwtSettings
- **`appsettings.Production.json`** - Configuration pour la production
- **`Program.cs`** - Mise à jour (JWT authentication, services, middleware)

### Documentation
- **`AUTHENTICATION.md`** - Guide complet d'utilisation de l'authentification
- **`create-auth-migration.sh`** - Script Linux/Mac pour créer la migration
- **`create-auth-migration.bat`** - Script Windows pour créer la migration
- **`GestionDeStock.API/auth.http`** - Fichier pour tester l'authentification

## 🔐 Modifications des contrôleurs existants

Ajout de l'attribut `[Authorize]` et du using `Microsoft.AspNetCore.Authorization` à:
- ✅ ProductsController
- ✅ CategoriesController
- ✅ OrdersController
- ✅ CustomersController
- ✅ SuppliersController
- ✅ InventoryController
- ✅ StockMovementController
- ✅ MovementsController
- ✅ ProvidesController
- ✅ EcomProductsController

**Note:** AuthController reste sans `[Authorize]` pour permettre l'enregistrement et la connexion.

## 🔑 Configuration JWT

### Variables de configuration (appsettings.json)
```json
"JwtSettings": {
  "Key": "your-super-secret-key-change-this-in-production-12345",
  "Issuer": "GestionDeStockAPI",
  "Audience": "GestionDeStockClient",
  "ExpirationHours": 24
}
```

### Middleware ajouté dans Program.cs
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { ... });

app.UseAuthentication();
```

## 📊 Structure de la base de données

Nouvelle table `Users`:
```
Id (int, primary key, auto-increment)
Email (string, unique, required)
Username (string, unique, required)
PasswordHash (string, required) - SHA-256
IsActive (bool, default: true)
CreatedAt (datetime)
UpdatedAt (datetime)
```

## 🚀 Prochaines étapes

### 1. Créer la migration
```bash
# Windows
create-auth-migration.bat

# Linux/Mac
chmod +x create-auth-migration.sh
./create-auth-migration.sh
```

Ou manuellement:
```bash
cd GestionDeStock.API
dotnet ef migrations add AddUserTable
dotnet ef database update
```

### 2. Tester l'authentification

**Enregistrement:**
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "username": "testuser",
    "password": "TestPassword123",
    "confirmPassword": "TestPassword123"
  }'
```

**Connexion:**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "TestPassword123"
  }'
```

### 3. Utiliser le token
Incluez le token dans l'en-tête `Authorization`:
```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
  http://localhost:5000/api/products
```

### 4. Tester dans Swagger
1. Accédez à `http://localhost:5000/swagger`
2. Cliquez sur "Authorize"
3. Entrez `Bearer YOUR_TOKEN`
4. Testez les routes protégées

## 🔒 Sécurité

### Points importants
✅ Authentification JWT obligatoire  
✅ Tokens expirables (24h par défaut)  
✅ Mots de passe hashés (SHA-256)  
✅ Validation d'entrée  
✅ Routes sensibles protégées  

### À améliorer en production
⚠️ Utiliser bcrypt au lieu de SHA-256  
⚠️ Implémenter un refresh token  
⚠️ Ajouter la confirmation d'email  
⚠️ Rate limiting sur les tentatives de connexion  
⚠️ Changer la clé JWT en variable d'environnement  

## 📝 Logs disponibles

L'AuthService log automatiquement:
- Enregistrement réussi
- Connexion réussie
- Erreurs d'authentification
- Tentatives avec email non existant

Consultez les logs pour déboguer les problèmes d'authentification.

## 🧪 Fichiers de test

- **`auth.http`** - Fichier pour tester avec REST Client (VS Code)
- **`GestionDeStock.API.http`** - Fichier HTTP existant

## ❓ Questions fréquentes

**Q: Comment changer la durée du token?**
R: Modifiez `JwtSettings:ExpirationHours` dans appsettings.json

**Q: Comment supprimer un utilisateur?**
R: Les utilisateurs ne sont pas supprimés, mais désactivés (`IsActive = false`)

**Q: Comment réinitialiser le mot de passe?**
R: À implémenter (Forgot password endpoint)

**Q: Puis-je utiliser plusieurs tokens?**
R: Oui, chaque nouvelle connexion génère un nouveau token

**Q: Le token est-il utilisable immédiatement?**
R: Oui, sans délai
