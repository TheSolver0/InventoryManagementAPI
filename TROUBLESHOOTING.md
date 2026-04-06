# 🐛 Troubleshooting - Problèmes courants et solutions

## 🔴 Erreurs de compilation

### 1. "The type or namespace name 'Pomelo' could not be found"

**Cause:** Les packages NuGet ne sont pas restaurés.

**Solution:**
```bash
cd GestionDeStock.API
dotnet restore
```

**Vérification:**
```bash
dotnet build
```

---

### 2. "The type or namespace name 'ServerVersion' does not exist"

**Cause:** Les packages NuGet ne sont pas restaurés correctement.

**Solution:**
```bash
dotnet clean
dotnet restore
dotnet build
```

---

### 3. "Missing using directive or assembly reference"

**Cause:** Un using statement est manquant.

**Solution:**
Assurez-vous que les fichiers modifiés ont:
```csharp
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
```

---

## 🔴 Erreurs de migration

### 1. "Table 'Users' doesn't exist"

**Cause:** La migration n'a pas été exécutée.

**Solution:**
```bash
# Windows
create-auth-migration.bat

# Linux/Mac
./create-auth-migration.sh

# Ou manuellement
dotnet ef migrations add AddUserTable
dotnet ef database update
```

---

### 2. "A column with name X already exists"

**Cause:** La migration a déjà été appliquée.

**Solution:**
```bash
# Vérifier le statut
dotnet ef migrations list

# La migration devrait déjà être appliquée
# Si c'est une erreur, supprimez et recommencez:
dotnet ef database update AddTimeStamp  # Revenir à la version précédente
dotnet ef migrations remove  # Supprimer la dernière migration
```

---

### 3. "Unable to connect to database"

**Cause:** La connexion à la base de données n'est pas configurée correctement.

**Vérifications:**
1. `appsettings.json` a-t-il une ConnectionString valide?
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Data Source=GestionDeStockAPP.db"
   }
   ```

2. En production MySQL, vérifiez:
   - L'hôte MySQL est accessible
   - Le user/password est correct
   - La base de données existe

3. En Docker, vérifiez:
   ```bash
   docker-compose ps
   docker-compose logs mysql
   ```

---

## 🔴 Erreurs d'authentification

### 1. "401 Unauthorized" sur tous les endpoints

**Causes possibles:**
1. Le token est manquant
2. Le token est expiré
3. Le token est invalide

**Solutions:**
```bash
# 1. Vérifier que vous envoyez le token
curl -H "Authorization: Bearer YOUR_TOKEN" http://localhost:5000/api/products

# 2. Obtenir un nouveau token
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123"}'

# 3. Tester avec cURL sans token (devrait être 401)
curl http://localhost:5000/api/products
# ← Doit retourner 401
```

---

### 2. "Email ou mot de passe incorrect"

**Causes:**
- Email n'existe pas
- Mot de passe incorrect

**Solutions:**
```bash
# 1. Vérifier que l'utilisateur existe (créer un nouveau s'il n'existe pas)
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email":"test@example.com",
    "username":"testuser",
    "password":"Test123",
    "confirmPassword":"Test123"
  }'

# 2. Essayer de se connecter avec les bons identifiants
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123"}'
```

---

### 3. "Le mot de passe doit contenir au moins 6 caractères"

**Cause:** Mot de passe trop court.

**Solution:**
```bash
# Utiliser un mot de passe d'au moins 6 caractères
{
  "password": "Test123",  # ✅ 8 caractères
  "confirmPassword": "Test123"
}
```

---

### 4. "Cet email est déjà utilisé"

**Cause:** L'email existe déjà dans la base de données.

**Solutions:**
1. Utiliser un email différent
2. Ou se connecter avec l'email existant:
   ```bash
   POST /api/auth/login
   {"email":"existing@example.com","password":"..."}
   ```

---

## 🔴 Erreurs Docker

### 1. "Connection refused" pour MySQL

**Cause:** Le conteneur MySQL n'est pas prêt.

**Solution:**
```bash
# Attendez que MySQL soit prêt (30 secondes)
docker-compose logs mysql

# Vérifiez que MySQL est en santé
docker-compose ps
# STATUS devrait être "healthy"

# Si problème persiste, redémarrez
docker-compose restart mysql
```

---

### 2. "Port 3306 already in use"

**Cause:** Un autre service utilise le port 3306.

**Solutions:**
1. Arrêtez l'autre service
2. Ou changez le port dans docker-compose.yml:
   ```yaml
   mysql:
     ports:
       - "3307:3306"  # Changez de 3306 à 3307
   ```

---

### 3. "Port 8080 already in use"

**Cause:** Un autre service utilise le port 8080.

**Solution:**
```bash
# Trouvez le processus qui utilise le port
lsof -i :8080  # Linux/Mac
netstat -ano | findstr :8080  # Windows

# Arrêtez-le ou utilisez un autre port:
docker-compose.yml:
  ports:
    - "8081:8080"  # Changez le port
```

---

### 4. "Migration fails in Docker"

**Cause:** La migration s'exécute avant que MySQL soit prêt.

**Solution:**
```bash
# Attendez que MySQL soit prêt
docker-compose up mysql -d
sleep 30

# Puis lancez l'API
docker-compose up api -d

# Ou exécutez la migration manuellement
docker exec gestion_de_stock_api dotnet ef database update
```

---

## 🔴 Erreurs Swagger

### 1. "The server returned an error"

**Causes:**
1. Swagger ne peut pas accéder à l'API
2. Le token est manquant ou invalide

**Solutions:**
```bash
1. Vérifiez que l'API fonctionne
   curl http://localhost:5000

2. Dans Swagger, cliquez sur "Authorize" et entrez:
   Bearer YOUR_TOKEN
```

---

### 2. "Path not found" dans Swagger

**Cause:** Les routes [Authorize] nécessitent un token.

**Solution:**
1. Obtenez un token: `POST /api/auth/login`
2. Cliquez sur "Authorize" en haut à droite
3. Entrez: `Bearer YOUR_TOKEN`
4. Maintenant, tous les endpoints sont accessibles

---

## 🔴 Erreurs d'intégration frontend

### 1. "CORS error: Access-Control-Allow-Origin"

**Cause:** CORS n'est pas configuré pour votre domaine.

**Solution dans Program.cs:**
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedFrontEnd", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000", "https://yourdomain.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
```

---

### 2. "Token not sent"

**Cause:** L'en-tête Authorization n'est pas envoyé.

**Solution en JavaScript:**
```javascript
const token = localStorage.getItem('token');

fetch('http://localhost:5000/api/products', {
  headers: {
    'Authorization': `Bearer ${token}`  // ← Ne pas oublier!
  }
})
```

---

## ✅ Vérifications de santé

### Vérifier que tout fonctionne

**1. L'API répond:**
```bash
curl http://localhost:5000
# Doit retourner: "Welcome to GestionDeStock API!"
```

**2. Swagger est accessible:**
```bash
curl http://localhost:5000/swagger
# Doit retourner la page HTML
```

**3. L'authentification fonctionne:**
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@example.com","password":"Test123"}'
# Doit retourner un token
```

**4. Les routes protégées nécessitent un token:**
```bash
# Sans token
curl http://localhost:5000/api/products
# Doit retourner 401

# Avec token
curl -H "Authorization: Bearer TOKEN" http://localhost:5000/api/products
# Doit retourner 200
```

---

## 📝 Logs et debugging

### Voir les logs de l'API

**Docker:**
```bash
docker-compose logs -f api

# Voir les logs MySQL
docker-compose logs -f mysql
```

**Visual Studio:**
```
View → Output
ou Debug → Windows → Output
```

**Terminal:**
```bash
dotnet run
# Les logs s'affichent directement
```

---

### Activer les logs détaillés

**appsettings.json:**
```json
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "Microsoft.AspNetCore": "Debug"
  }
}
```

---

## 🔧 Solutions rapides

| Problème | Commande |
|----------|----------|
| Les packages ne se restaurent pas | `dotnet clean && dotnet restore` |
| La migration échoue | `create-auth-migration.bat` ou `.sh` |
| Migration non appliquée | `dotnet ef database update` |
| Besoin de reconstruire | `dotnet build -c Release` |
| Besoin de nettoyer | `dotnet clean` |
| Docker à refaire | `docker-compose down -v && docker-compose up -d` |
| API ne répond pas | `docker-compose restart api` |
| MySQL déconnecté | `docker-compose restart mysql` |

---

## 🆘 Si rien ne fonctionne

### 1. Commencez par le début
```bash
# Nettoyez
dotnet clean
rm -rf bin obj

# Restaurez
dotnet restore

# Compilez
dotnet build

# Exécutez la migration
create-auth-migration.bat

# Lancez
dotnet run
```

### 2. Consultez les logs
```bash
# Voir les erreurs
dotnet run 2>&1 | grep -i error

# Ou dans Docker
docker-compose logs api | grep -i error
```

### 3. Vérifiez la configuration
```bash
# Vérifier appsettings.json
cat GestionDeStock.API/appsettings.json

# Vérifier la connexion BD
# Dans appsettings.json, la clé "ConnectionStrings"
```

---

## 📞 Support et ressources

### Fichiers d'aide
- [README_AUTHENTICATION.md](README_AUTHENTICATION.md) - Guide principal
- [QUICKSTART_AUTH.md](QUICKSTART_AUTH.md) - Démarrage rapide
- [AUTH_INDEX.md](AUTH_INDEX.md) - Index de documentation

### Documentation externe
- [ASP.NET Core Troubleshooting](https://docs.microsoft.com/aspnet/core/troubleshooting)
- [Docker Troubleshooting](https://docs.docker.com/config/containers/logging/)
- [JWT Debugging](https://jwt.io/)

### Tester en ligne
- **JWT Decoder:** https://jwt.io/ (Copiez-collez votre token)

---

**Vous n'avez pas trouvé votre problème?**
→ Consultez [AUTH_INDEX.md](AUTH_INDEX.md) pour les autres documents! 📖
