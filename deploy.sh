#!/bin/bash
# Inventory Management API - Deployment Script

set -e

echo "🚀 Starting Inventory Management API deployment..."

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo -e "${YELLOW}Docker is not installed. Installing Docker...${NC}"
    curl -fsSL https://get.docker.com -o get-docker.sh
    sudo sh get-docker.sh
    rm get-docker.sh
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    echo -e "${YELLOW}Docker Compose is not installed. Installing Docker Compose...${NC}"
    sudo curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    sudo chmod +x /usr/local/bin/docker-compose
fi

# Create .env file if it doesn't exist
if [ ! -f .env ]; then
    echo -e "${BLUE}Creating .env file from .env.example...${NC}"
    cp .env.example .env
    echo -e "${YELLOW}⚠️  Please edit .env file with your configuration before continuing${NC}"
    exit 1
fi

# Create uploads directory
mkdir -p uploads

echo -e "${BLUE}Building Docker images...${NC}"
docker-compose build

echo -e "${BLUE}Starting services...${NC}"
docker-compose up -d

echo -e "${BLUE}Waiting for MySQL to be ready...${NC}"
sleep 30

echo -e "${BLUE}Checking service health...${NC}"
docker-compose ps

echo ""
echo -e "${GREEN}✅ Deployment completed successfully!${NC}"
echo ""
echo -e "${BLUE}API Information:${NC}"
echo "  - API URL: http://localhost:8080"
echo "  - Swagger UI: http://localhost:8080/swagger"
echo "  - MySQL Port: 3306"
echo ""
echo -e "${BLUE}Useful commands:${NC}"
echo "  - View logs: docker-compose logs -f api"
echo "  - Stop services: docker-compose down"
echo "  - Restart services: docker-compose restart"
echo ""
