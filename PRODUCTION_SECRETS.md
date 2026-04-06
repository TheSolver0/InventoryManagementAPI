# Variables d'environnement pour la production

## 🔐 Configuration JWT en production

### 1. Générer une clé JWT sécurisée

**Avec PowerShell:**
```powershell
$key = [Convert]::ToBase64String($(Get-Random -InputObject $(1..255) -Count 32))
Write-Output "Jwt__Key=$key"
```

**Avec Linux/Mac:**
```bash
openssl rand -base64 32
```

**Résultat:**
```
Jwt__Key=your-random-base64-string-of-32-characters
```

## 2. Configuration Docker

Mettez à jour le fichier `.env` avec:

```env
# JWT Configuration
JWT_KEY=your-generated-secret-key-min-32-chars
JWT_ISSUER=GestionDeStockAPI
JWT_AUDIENCE=GestionDeStockClient
JWT_EXPIRATION_HOURS=24

# Database Configuration
DB_ROOT_PASSWORD=secure-root-password-here
DB_NAME=GestionDeStockDB
DB_USER=gestionuser
DB_PASSWORD=secure-password-here

# Application
ASPNETCORE_ENVIRONMENT=Production
```

## 3. Mise à jour du docker-compose.yml

Ajoutez les variables d'environnement JWT au service API:

```yaml
api:
  build:
    context: .
    dockerfile: Dockerfile
  container_name: gestion_de_stock_api
  environment:
    - ASPNETCORE_ENVIRONMENT=Production
    - ASPNETCORE_URLS=http://+:8080
    - ConnectionStrings__DefaultConnection=Server=mysql;Port=3306;Database=${DB_NAME};Uid=${DB_USER};Pwd=${DB_PASSWORD};
    - JwtSettings__Key=${JWT_KEY}
    - JwtSettings__Issuer=${JWT_ISSUER}
    - JwtSettings__Audience=${JWT_AUDIENCE}
    - JwtSettings__ExpirationHours=${JWT_EXPIRATION_HOURS}
  ports:
    - "8080:8080"
  depends_on:
    mysql:
      condition: service_healthy
  networks:
    - gestion_network
  restart: unless-stopped
  volumes:
    - ./uploads:/app/wwwroot/uploads
```

## 4. Mise à jour du appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=mysql;Port=3306;Database=GestionDeStockDB;Uid=gestionuser;Pwd=gestionpassword;"
  },
  "JwtSettings": {
    "Key": "placeholder-will-be-overridden-by-env-var",
    "Issuer": "GestionDeStockAPI",
    "Audience": "GestionDeStockClient",
    "ExpirationHours": 24
  }
}
```

## 5. Checkliste de sécurité production

### ✅ À faire avant de déployer

- [ ] Générer une clé JWT sécurisée (minimum 32 caractères)
- [ ] Utiliser des mots de passe forts pour MySQL
- [ ] Activer HTTPS/SSL avec Let's Encrypt
- [ ] Configurer CORS pour les domaines autorisés
- [ ] Mettre à jour les variables d'environnement
- [ ] Tester l'authentification en production
- [ ] Configurer des logs pour auditer les accès
- [ ] Implémenter un WAF (pare-feu applicatif)
- [ ] Mettre en place des backups réguliers
- [ ] Configurer un monitoring

### ⚠️ Ne pas faire

- ❌ Committer les mots de passe en Git
- ❌ Utiliser la clé JWT par défaut
- ❌ Laisser les ports MySQL exposés publiquement
- ❌ Désactiver HTTPS
- ❌ Stocker les secrets en clair

## 6. Déploiement avec Docker Compose

### Créer le fichier .env
```bash
cp .env.example .env
# Éditer .env avec vos secrets
```

### Lancer les services
```bash
docker-compose up -d
```

### Vérifier les logs
```bash
docker-compose logs -f api
```

## 7. Variables d'environnement sur le serveur

Si vous ne déployez pas avec Docker, définissez les variables d'environnement:

### Linux/Mac
```bash
export JwtSettings__Key=your-secret-key
export JwtSettings__Issuer=GestionDeStockAPI
export JwtSettings__Audience=GestionDeStockClient
export JwtSettings__ExpirationHours=24
export ConnectionStrings__DefaultConnection="Server=localhost;Database=GestionDeStockDB;..."

dotnet GestionDeStock.API.dll
```

### Windows (PowerShell)
```powershell
$env:JwtSettings__Key="your-secret-key"
$env:JwtSettings__Issuer="GestionDeStockAPI"
$env:JwtSettings__Audience="GestionDeStockClient"
$env:JwtSettings__ExpirationHours="24"
$env:ConnectionStrings__DefaultConnection="Server=localhost;Database=GestionDeStockDB;..."

dotnet GestionDeStock.API.dll
```

## 8. Nginx reverse proxy

Fichier `/etc/nginx/sites-available/api`:

```nginx
server {
    listen 80;
    server_name api.yourdomain.com;

    # Redirect HTTP to HTTPS
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name api.yourdomain.com;

    # SSL Configuration
    ssl_certificate /etc/letsencrypt/live/api.yourdomain.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.yourdomain.com/privkey.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    ssl_ciphers HIGH:!aNULL:!MD5;

    # Proxy settings
    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        # JWT Token headers
        proxy_set_header Authorization $http_authorization;
    }
}
```

## 9. Activation HTTPS avec Let's Encrypt

```bash
sudo apt install certbot python3-certbot-nginx -y
sudo certbot --nginx -d api.yourdomain.com

# Auto-renouvellement
sudo systemctl enable certbot.timer
sudo systemctl start certbot.timer
```

## 10. Rotation des secrets

### Changer la clé JWT
1. Générer une nouvelle clé
2. Mettre à jour la variable d'environnement
3. Redémarrer l'application
4. Les tokens existants resteront valides jusqu'à expiration (24h)

### Changer le mot de passe MySQL
```bash
# Dans le conteneur MySQL
docker exec -it gestion_de_stock_mysql mysql -u root -p

ALTER USER 'gestionuser'@'%' IDENTIFIED BY 'new-secure-password';
FLUSH PRIVILEGES;
```

## 📊 Monitoring

### Logs d'authentification
```bash
docker-compose logs api | grep -i "auth\|login\|register"
```

### Métriques à surveiller
- Nombre de tentatives de login échouées
- Tokens expirés
- Erreurs d'authentification
- Performances des migrations

---

**Important:** Sauvegardez vos clés et secrets dans un gestionnaire de secrets sécurisé! 🔐
