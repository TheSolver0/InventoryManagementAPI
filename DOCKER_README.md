# Configuration Docker pour GestionDeStock API

## 📋 Fichiers créés

### Fichiers Docker
- **`Dockerfile`**: Configuration multi-étape pour construire l'image .NET
- **`docker-compose.yml`**: Orchestration de l'API et de MySQL
- **`.dockerignore`**: Fichiers à exclure de l'image Docker
- **`init.sql`**: Scripts d'initialisation MySQL (optionnel)

### Fichiers de configuration
- **`.env.example`**: Variables d'environnement (template)
- **`appsettings.Production.json`**: Configuration de production

### Fichiers de déploiement
- **`DEPLOYMENT.md`**: Guide complet de déploiement
- **`deploy.sh`**: Script de déploiement automatisé (Linux/Mac)
- **`deploy.bat`**: Script de déploiement automatisé (Windows)

## 🚀 Démarrage rapide

### Sur Linux/Mac
```bash
chmod +x deploy.sh
./deploy.sh
```

### Sur Windows
```bash
deploy.bat
```

## 🔧 Configuration manuelle

### 1. Créer le fichier .env
```bash
cp .env.example .env
```

Éditer `.env` avec vos identifiants de base de données

### 2. Construire et lancer
```bash
docker-compose up -d
```

### 3. Vérifier le statut
```bash
docker-compose ps
```

## 📊 Structure Docker

```
API Container
├── Framework: .NET 9.0
├── Port: 8080
└── Environment: Production

MySQL Container
├── Version: 8.0
├── Port: 3306
├── Volume: mysql_data (persistant)
└── Network: gestion_network
```

## 🔌 Configurations de connexion

### Depuis l'API vers MySQL
```
Server: mysql
Port: 3306
Database: GestionDeStockDB
User: gestionuser
Password: (défini dans .env)
```

### Depuis l'extérieur du conteneur
```
Server: localhost (ou IP du serveur)
Port: 3306
Database: GestionDeStockDB
User: gestionuser
Password: (défini dans .env)
```

## 📝 Changements de code

### Modification de la base de données

Le code est configuré pour:
- **Développement (local)**: SQLite (GestionDeStockAPP.db)
- **Production (Docker)**: MySQL via variables d'environnement

**Fichier clé**: `GestionDeStock.API/Program.cs`

```csharp
// Lecture de la connexion MySQL depuis appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    // Utilise SQLite en développement
    builder.Services.AddDbContext<AppDbContext>(options => 
        options.UseSqlite("Data Source=GestionDeStockAPP.db"));
}
else
{
    // Utilise MySQL en production
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(connectionString, 
            ServerVersion.AutoDetect(connectionString)));
}
```

## 📦 Dépendances NuGet ajoutées

```xml
<PackageReference Include="Pomelo.EntityFrameworkCore.MySql" Version="8.0.2" />
```

Cette package permet à Entity Framework Core de communiquer avec MySQL.

## 🛠️ Commandes utiles

### Logs
```bash
# Logs de l'API
docker-compose logs -f api

# Logs de MySQL
docker-compose logs -f mysql

# Tous les logs
docker-compose logs -f
```

### Accès à MySQL
```bash
docker exec -it gestion_de_stock_mysql mysql -u root -p
```

### Exécuter une commande dans le conteneur API
```bash
docker exec gestion_de_stock_api dotnet <command>
```

### Restart des services
```bash
docker-compose restart
```

### Arrêter les services
```bash
docker-compose down
```

### Supprimer les volumes (⚠️ supprime les données)
```bash
docker-compose down -v
```

## 🔒 Sécurité

### Avant de déployer en production

1. **Changez les mots de passe** dans `.env`
2. **Utilisez des identifiants forts** (minimum 12 caractères)
3. **Limitez l'accès MySQL** au réseau interne (port 3306)
4. **Activez HTTPS** avec un certificat SSL
5. **Sauvegardez régulièrement** la base de données
6. **Mettez à jour** les images Docker régulièrement

### Variables d'environnement sensibles

Ne commitez jamais le fichier `.env` dans Git. Il est inclus dans `.gitignore`.

## 📈 Performance

### Volumes
- `mysql_data`: Stockage persistant de la base de données
- `uploads`: Répertoire pour les fichiers de l'application

### Réseau
- `gestion_network`: Réseau privé pour la communication entre conteneurs
- L'API communique avec MySQL via le nom de service `mysql` (résolution DNS interne)

## 🐛 Dépannage

### "Connection refused" pour MySQL
```bash
# Vérifier que MySQL est en cours d'exécution
docker-compose ps

# Vérifier les logs
docker-compose logs mysql

# Attendre que MySQL soit prêt
docker-compose restart api
```

### "Port already in use"
```bash
# Changer les ports dans docker-compose.yml
# Cherchez les sections "ports:"
```

### Migrations de base de données échouent
```bash
# Reconstruire l'image
docker-compose up -d --build

# Vérifier les logs
docker-compose logs api
```

## 📚 Ressources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [Pomelo EntityFrameworkCore MySQL](https://github.com/PomeloFoundation/Pomelo.EntityFrameworkCore.MySql)
- [ASP.NET Core on Docker](https://docs.microsoft.com/en-us/dotnet/core/docker/introduction)
