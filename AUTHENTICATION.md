# Guide d'Authentification - GestionDeStock API

## 📋 Vue d'ensemble

L'authentification a été implémentée avec JWT (JSON Web Tokens). Les utilisateurs doivent se connecter ou s'enregistrer pour accéder aux routes protégées.

## 🔐 Routes d'authentification (Sans authentification requise)

### 1. Enregistrement
```
POST /api/auth/register
```

**Corps de la requête:**
```json
{
  "email": "user@example.com",
  "username": "johndoe",
  "password": "SecurePassword123",
  "confirmPassword": "SecurePassword123"
}
```

**Réponse réussie (200):**
```json
{
  "success": true,
  "message": "Enregistrement réussi.",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "email": "user@example.com",
    "username": "johndoe",
    "isActive": true,
    "createdAt": "2025-04-06T10:30:00Z"
  }
}
```

### 2. Connexion
```
POST /api/auth/login
```

**Corps de la requête:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123"
}
```

**Réponse réussie (200):**
```json
{
  "success": true,
  "message": "Connexion réussie.",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "email": "user@example.com",
    "username": "johndoe",
    "isActive": true,
    "createdAt": "2025-04-06T10:30:00Z"
  }
}
```

## 🛡️ Routes protégées (Authentification requise)

Les routes suivantes nécessitent un token JWT valide:

- **Products** - `GET/POST/PUT/DELETE /api/products`
- **Categories** - `GET/POST/PUT/DELETE /api/categories`
- **Orders** - `GET/POST/PUT/DELETE /api/orders`
- **Customers** - `GET/POST/PUT/DELETE /api/customers`
- **Suppliers** - `GET/POST/PUT/DELETE /api/suppliers`
- **Inventory** - `GET/POST /api/inventory`
- **Movements** - `GET/POST /api/movements`
- **StockMovements** - `GET/POST /api/stockmovement`
- **Provides** - `GET/POST/PUT/DELETE /api/provides`
- **EcomProducts** - `GET/POST /api/ecomproducts`

## 🔑 Comment utiliser le token

### 1. Obtenir le token
Utilisez les routes `/api/auth/login` ou `/api/auth/register` pour obtenir un token.

### 2. Envoyer le token avec les requêtes
Incluez le token dans l'en-tête `Authorization` de vos requêtes:

**Avec cURL:**
```bash
curl -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  http://localhost:8080/api/products
```

**Avec JavaScript/Fetch:**
```javascript
const token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";

fetch('http://localhost:8080/api/products', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${token}`,
    'Content-Type': 'application/json'
  }
})
.then(response => response.json())
.then(data => console.log(data));
```

**Avec Axios:**
```javascript
const token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";

axios.get('http://localhost:8080/api/products', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
})
.then(response => console.log(response.data));
```

**Avec Swagger/OpenAPI:**
1. Cliquez sur le bouton "Authorize" en haut à droite
2. Entrez `Bearer YOUR_TOKEN_HERE`
3. Cliquez sur "Authorize"
4. Maintenant, vous pouvez tester les routes protégées

## 🔧 Configuration JWT

La configuration JWT se trouve dans `appsettings.json`:

```json
"JwtSettings": {
  "Key": "your-super-secret-key-change-this-in-production-12345",
  "Issuer": "GestionDeStockAPI",
  "Audience": "GestionDeStockClient",
  "ExpirationHours": 24
}
```

### ⚠️ Configuration en production

**IMPORTANT:** Changez la clé JWT en production!

1. Utilisez une clé secrète forte (au moins 32 caractères)
2. Stockez-la dans les variables d'environnement
3. Mettez à jour `appsettings.Production.json`

**Avec Docker/Variables d'environnement:**
```bash
# Dans le Dockerfile ou docker-compose.yml
ENV JwtSettings__Key=your-production-secret-key-min-32-chars-long
```

## 📊 Structure de la table Users

```sql
CREATE TABLE Users (
  Id INT PRIMARY KEY AUTO_INCREMENT,
  Email VARCHAR(255) NOT NULL UNIQUE,
  Username VARCHAR(255) NOT NULL UNIQUE,
  PasswordHash VARCHAR(255) NOT NULL,
  IsActive BOOLEAN DEFAULT true,
  CreatedAt DATETIME NOT NULL,
  UpdatedAt DATETIME NOT NULL
);
```

## 🚀 Première utilisation

### 1. Créer la migration
```bash
dotnet ef migrations add AddUserTable
```

### 2. Appliquer la migration
```bash
dotnet ef database update
```

### 3. Tester l'authentification

**Enregistrement:**
```bash
curl -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "username": "testuser",
    "password": "Test@1234",
    "confirmPassword": "Test@1234"
  }'
```

**Connexion:**
```bash
curl -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test@1234"
  }'
```

**Utiliser le token:**
```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
  http://localhost:8080/api/products
```

## ✅ Codes de réponse

| Code | Signification |
|------|---|
| 200 | Succès |
| 400 | Requête invalide (email/password déjà utilisé, mot de passe trop court, etc.) |
| 401 | Non autorisé (token manquant, invalide ou expiré) |
| 403 | Interdit |
| 404 | Non trouvé |
| 500 | Erreur serveur |

## 🔐 Sécurité - Bonnes pratiques

1. **Mots de passe forts:** Minimum 6 caractères (recommandé 12+)
2. **HTTPS obligatoire:** Utilisez toujours HTTPS en production
3. **Token expirant:** Par défaut 24h (configurable)
4. **Clé secrète robuste:** Minimum 32 caractères en production
5. **Hash des mots de passe:** Utilisé SHA-256 + stockage sécurisé
6. **Validation des entrées:** Validations côté client et serveur
7. **CORS configuré:** Restreindre aux domaines autorisés si nécessaire

## 🔄 Rafraîchissement du token

Actuellement, le token expire après 24 heures. L'utilisateur doit se reconnecter pour en obtenir un nouveau.

**Pour implémenter un refresh token à l'avenir:**
1. Ajouter une table `RefreshTokens`
2. Créer une route `/api/auth/refresh`
3. Valider le refresh token et émettre un nouveau JWT

## 🐛 Troubleshooting

### Token expiré
**Erreur:** `401 Unauthorized`
**Solution:** Reconnectez-vous avec `/api/auth/login`

### Format de token invalide
**Erreur:** `401 Unauthorized`
**Solution:** Assurez-vous que le token commence par "Bearer "

### Mot de passe trop court
**Erreur:** `400 Bad Request`
**Solution:** Utilisez un mot de passe d'au moins 6 caractères

### Email déjà utilisé
**Erreur:** `400 Bad Request`
**Solution:** Utilisez un email différent ou connectez-vous

## 📚 Ressources

- [JWT (JSON Web Tokens)](https://jwt.io/)
- [ASP.NET Core Authentication](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/)
- [Bearer Token](https://tools.ietf.org/html/rfc6750)
