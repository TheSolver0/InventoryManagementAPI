# Script d'initialisation de la migration pour Docker

## Avec Docker Compose

Si vous utilisez Docker Compose, la migration s'exécutera automatiquement au démarrage si vous avez configuré les commandes appropriées.

## Migration manuelle en Docker

### 1. Accéder au conteneur API
```bash
docker exec -it gestion_de_stock_api bash
```

### 2. Exécuter la migration
```bash
dotnet ef migrations add AddUserTable
dotnet ef database update
```

### 3. Quitter le conteneur
```bash
exit
```

## Alternative: Mise à jour du Dockerfile

Si vous voulez que la migration s'exécute automatiquement au démarrage, modifiez le Dockerfile:

```dockerfile
# Avant la dernière ligne (ENTRYPOINT)

# Run migrations
RUN dotnet ef database update || true

# Run the application
ENTRYPOINT ["dotnet", "GestionDeStock.API.dll"]
```

## Vérification

Pour vérifier que la migration a été appliquée:

```bash
# Accéder à MySQL
docker exec -it gestion_de_stock_mysql mysql -u gestionuser -p -D GestionDeStockDB

# Lister les tables
SHOW TABLES;

# Vérifier la table Users
DESCRIBE Users;
```

## En cas d'erreur

Si la migration échoue:

1. **Vérifier les logs:**
   ```bash
   docker-compose logs api
   ```

2. **Supprimer et recréer les volumes (⚠️ perte de données):**
   ```bash
   docker-compose down -v
   docker-compose up -d --build
   ```

3. **Vérifier la connexion MySQL:**
   ```bash
   docker-compose logs mysql
   ```
