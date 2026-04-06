# 🚀 Authentification - Guide de démarrage rapide

## ⚡ 5 minutes pour commencer

### Étape 1: Créer la migration (une seule fois)

**Windows:**
```bash
create-auth-migration.bat
```

**Linux/Mac:**
```bash
chmod +x create-auth-migration.sh
./create-auth-migration.sh
```

### Étape 2: Enregistrer un utilisateur

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "username": "myuser",
    "password": "SecurePass123",
    "confirmPassword": "SecurePass123"
  }'
```

**Vous recevrez une réponse comme:**
```json
{
  "success": true,
  "message": "Enregistrement réussi.",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "email": "user@example.com",
    "username": "myuser",
    "isActive": true,
    "createdAt": "2025-04-06T10:30:00Z"
  }
}
```

### Étape 3: Copier le token

Copiez la valeur du champ `token` (sans les guillemets).

### Étape 4: Utiliser le token

**Avec cURL:**
```bash
curl -H "Authorization: Bearer YOUR_TOKEN_HERE" \
  http://localhost:5000/api/products
```

**Ou dans Swagger:**
1. Allez sur `http://localhost:5000/swagger`
2. Cliquez sur "🔒 Authorize"
3. Entrez: `Bearer YOUR_TOKEN_HERE`
4. Testez les endpoints

## 📝 Cas d'usage courants

### Obtenir un nouveau token
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123"
  }'
```

### Créer un produit (authentifié)
```bash
curl -X POST http://localhost:5000/api/products \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Mon Produit",
    "price": 99.99,
    "quantity": 50,
    "categoryId": 1
  }'
```

### Récupérer les commandes (authentifié)
```bash
curl -H "Authorization: Bearer YOUR_TOKEN" \
  http://localhost:5000/api/orders
```

## 🔑 Clés importantes

| Concept | Description |
|---------|---|
| **Register** | Créer un nouvel utilisateur et obtenir un token |
| **Login** | Connecter un utilisateur et obtenir un token |
| **Token** | Clé JWT à utiliser pour les requêtes protégées |
| **Bearer** | Type d'authentification HTTP standard |
| **Expiration** | Token valide 24h par défaut |

## ✅ Routes sans authentification requise
- `POST /api/auth/register` - Enregistrement
- `POST /api/auth/login` - Connexion
- `GET /` - Page d'accueil

## 🛡️ Routes avec authentification requise (Bearer token)
- `GET /api/products` - Lister les produits
- `POST /api/products` - Créer un produit
- `GET /api/orders` - Lister les commandes
- `POST /api/orders` - Créer une commande
- Et tous les autres endpoints

## 🆘 Si ça ne fonctionne pas

**Erreur: `"The type or namespace name 'Pomelo' could not be found"`**
→ Restaurez les packages: `dotnet restore`

**Erreur: `401 Unauthorized`**
→ Votre token a expiré ou est manquant. Reconnectez-vous.

**Erreur: `Table 'Users' doesn't exist`**
→ Exécutez la migration: `create-auth-migration.bat` ou `.sh`

**La migration échoue**
→ Vérifiez la connexion à la base de données dans `appsettings.json`

## 📚 Documentation complète

Pour plus de détails, consultez:
- [AUTHENTICATION.md](AUTHENTICATION.md) - Guide complet d'authentification
- [AUTHENTICATION_IMPLEMENTATION.md](AUTHENTICATION_IMPLEMENTATION.md) - Détails techniques
- [MIGRATION_DOCKER.md](MIGRATION_DOCKER.md) - Migration en Docker

## 💡 Astuces

**Pour tester rapidement:**
- Utilisez le fichier `auth.http` avec l'extension REST Client de VS Code
- Ou visitez `http://localhost:5000/swagger` pour une UI interactive

**Pour générer plusieurs tokens:**
- Chaque utilisateur peut avoir plusieurs tokens (une par session)
- Les anciens tokens restent valides jusqu'à expiration

**Pour sécuriser en production:**
- Changez la clé JWT dans `appsettings.Production.json`
- Utilisez HTTPS obligatoirement
- Stockez la clé en variable d'environnement

---

**Prêt à commencer?** Exécutez `create-auth-migration.bat` et commencez à tester! 🎉
