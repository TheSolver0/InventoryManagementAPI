# Guide de Déploiement avec Docker

## Prérequis
- Docker et Docker Compose installés sur votre VPS
- Git (pour cloner le dépôt)

## Configuration initiale

### 1. Préparer le serveur
```bash
# Mettre à jour le système
sudo apt update && sudo apt upgrade -y

# Installer Docker
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Installer Docker Compose
sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
sudo chmod +x /usr/local/bin/docker-compose
```

### 2. Cloner le projet
```bash
cd /home/app
git clone <your-repo-url> inventory-api
cd inventory-api
```

### 3. Configurer les variables d'environnement
```bash
cp .env.example .env
```

Éditer le fichier `.env` et modifier les identifiants MySQL:
```bash
DB_ROOT_PASSWORD=votreMotDePasseSecurise
DB_NAME=GestionDeStockDB
DB_USER=gestionuser
DB_PASSWORD=votreMotDePasseSecurise
ASPNETCORE_ENVIRONMENT=Production
```

## Déploiement

### 1. Construire et lancer les conteneurs
```bash
docker-compose up -d
```

### 2. Vérifier le statut
```bash
docker-compose ps
```

Vous devriez voir 2 services en cours d'exécution: `mysql` et `api`

### 3. Consulter les logs
```bash
# Logs de l'API
docker-compose logs -f api

# Logs de MySQL
docker-compose logs -f mysql
```

### 4. Vérifier que l'API fonctionne
```bash
curl http://localhost:8080
```

## Accès à l'API

- **API Documentation (Swagger)**: `http://your-vps-ip:8080/swagger`
- **API Endpoint**: `http://your-vps-ip:8080`

## Gestion de la base de données

### Accéder à MySQL
```bash
docker exec -it gestion_de_stock_mysql mysql -u root -p
```

### Sauvegarde de la base de données
```bash
docker exec gestion_de_stock_mysql mysqldump -u root -p GestionDeStockDB > backup.sql
```

### Restaurer une sauvegarde
```bash
docker exec -i gestion_de_stock_mysql mysql -u root -p GestionDeStockDB < backup.sql
```

## Arrêter et redémarrer les services

### Arrêter
```bash
docker-compose down
```

### Redémarrer
```bash
docker-compose restart
```

### Reconstruire après des changements de code
```bash
docker-compose up -d --build
```

## Proxy inverse avec Nginx (optionnel mais recommandé)

### Installation d'Nginx
```bash
sudo apt install nginx -y
```

### Configuration Nginx
Créer `/etc/nginx/sites-available/inventory-api`:

```nginx
server {
    listen 80;
    server_name your-domain.com;

    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### Activer le site
```bash
sudo ln -s /etc/nginx/sites-available/inventory-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

## SSL avec Let's Encrypt (recommandé)

```bash
sudo apt install certbot python3-certbot-nginx -y
sudo certbot --nginx -d your-domain.com
```

## Troubleshooting

### L'API ne se connecte pas à MySQL
1. Vérifier que MySQL est en cours d'exécution: `docker-compose ps`
2. Vérifier les logs: `docker-compose logs mysql`
3. Vérifier la chaîne de connexion dans `.env`

### La migration de la base de données échoue
1. Vérifier les logs: `docker-compose logs api`
2. Arrêter et reconstruire: `docker-compose down && docker-compose up -d --build`

### Espace disque insuffisant
```bash
docker system prune -a
docker volume prune
```

## Mise à jour du code

```bash
git pull origin main
docker-compose up -d --build
```

## Fichiers importants

- `Dockerfile`: Configuration pour construire l'image Docker
- `docker-compose.yml`: Orchestration des conteneurs
- `.env`: Variables d'environnement (à créer depuis .env.example)
- `appsettings.Production.json`: Configuration de production

## Notes de sécurité

1. **Changez les mots de passe par défaut** dans le fichier `.env`
2. **Utilisez un pare-feu** pour restreindre l'accès au port 3306 (MySQL)
3. **Activez HTTPS** avec Let's Encrypt
4. **Sauvegardez régulièrement** votre base de données
5. **Maintenez à jour** Docker et les images des conteneurs
