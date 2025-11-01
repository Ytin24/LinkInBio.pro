# Деплой LinkInBio.pro

Инструкции по деплою на различные платформы.

## 🐳 Docker Deployment

### Production Docker Compose

Создайте `docker-compose.prod.yml`:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    restart: always
    environment:
      POSTGRES_DB: ${DB_NAME}
      POSTGRES_USER: ${DB_USER}
      POSTGRES_PASSWORD: ${DB_PASSWORD}
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - backend

  backend:
    build:
      context: ./LinkInBio.Backend
      dockerfile: Dockerfile
    restart: always
    environment:
      DATABASE_URL: "Host=postgres;Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}"
      JWT_SECRET: ${JWT_SECRET}
      ASPNETCORE_ENVIRONMENT: Production
      ASPNETCORE_URLS: http://0.0.0.0:5000
    ports:
      - "5000:5000"
    depends_on:
      - postgres
    networks:
      - backend

  nginx:
    image: nginx:alpine
    restart: always
    ports:
      - "80:80"
      - "443:443"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
      - ./ssl:/etc/nginx/ssl:ro
    depends_on:
      - backend
    networks:
      - backend

networks:
  backend:

volumes:
  postgres_data:
```

### Nginx Configuration

`nginx.conf`:

```nginx
events {
    worker_connections 1024;
}

http {
    upstream backend {
        server backend:5000;
    }

    server {
        listen 80;
        server_name linkinbio.pro www.linkinbio.pro;

        # Redirect HTTP to HTTPS
        return 301 https://$server_name$request_uri;
    }

    server {
        listen 443 ssl http2;
        server_name linkinbio.pro www.linkinbio.pro;

        ssl_certificate /etc/nginx/ssl/cert.pem;
        ssl_certificate_key /etc/nginx/ssl/key.pem;

        # SSL configuration
        ssl_protocols TLSv1.2 TLSv1.3;
        ssl_ciphers HIGH:!aNULL:!MD5;

        location / {
            proxy_pass http://backend;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection 'upgrade';
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
            proxy_cache_bypass $http_upgrade;
        }
    }
}
```

### Запуск Production

```bash
# 1. Создать .env файл
cat > .env << EOF
DB_NAME=linkinbio_prod
DB_USER=linkinbio_user
DB_PASSWORD=$(openssl rand -base64 32)
JWT_SECRET=$(openssl rand -base64 64)
EOF

# 2. Запустить
docker-compose -f docker-compose.prod.yml up -d

# 3. Проверить логи
docker-compose -f docker-compose.prod.yml logs -f
```

---

## ☁️ Облачные платформы

### Railway

1. Создать аккаунт на [Railway.app](https://railway.app)
2. Установить Railway CLI:
```bash
npm i -g @railway/cli
railway login
```

3. Инициализировать проект:
```bash
railway init
```

4. Добавить PostgreSQL:
```bash
railway add --plugin postgresql
```

5. Настроить переменные окружения:
```bash
railway variables set JWT_SECRET=$(openssl rand -base64 64)
railway variables set ASPNETCORE_ENVIRONMENT=Production
```

6. Деплой:
```bash
railway up
```

### Heroku

```bash
# 1. Создать приложение
heroku create linkinbio-pro

# 2. Добавить PostgreSQL
heroku addons:create heroku-postgresql:hobby-dev

# 3. Настроить buildpack
heroku buildpacks:set https://github.com/jincod/dotnetcore-buildpack

# 4. Переменные окружения
heroku config:set JWT_SECRET=$(openssl rand -base64 64)
heroku config:set ASPNETCORE_ENVIRONMENT=Production

# 5. Деплой
git push heroku main

# 6. Применить миграции
heroku pg:psql < LinkInBio.Backend/Database/Migrations.sql
```

### DigitalOcean App Platform

1. Создать `app.yaml`:

```yaml
name: linkinbio-pro
region: fra

databases:
  - name: db
    engine: PG
    production: true
    version: "15"

services:
  - name: api
    github:
      repo: yourusername/LinkInBio.pro
      branch: main
      deploy_on_push: true

    build_command: |
      cd LinkInBio.Backend
      dotnet publish -c Release -o out

    run_command: dotnet LinkInBio.Backend/out/LinkInBio.Backend.dll

    envs:
      - key: DATABASE_URL
        scope: RUN_TIME
        value: ${db.DATABASE_URL}
      - key: JWT_SECRET
        scope: RUN_TIME
        value: YOUR_JWT_SECRET
      - key: ASPNETCORE_ENVIRONMENT
        value: Production

    http_port: 5000

    instance_count: 1
    instance_size_slug: basic-xxs
```

2. Деплой:
```bash
doctl apps create --spec app.yaml
```

---

## 🖥️ VPS Deployment (Ubuntu)

### Подготовка сервера

```bash
# 1. Обновить систему
sudo apt update && sudo apt upgrade -y

# 2. Установить .NET SDK
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x ./dotnet-install.sh
./dotnet-install.sh --channel 8.0

# 3. Установить PostgreSQL
sudo apt install postgresql postgresql-contrib -y

# 4. Установить Nginx
sudo apt install nginx -y

# 5. Установить certbot (для SSL)
sudo apt install certbot python3-certbot-nginx -y
```

### Настройка PostgreSQL

```bash
# Создать пользователя и БД
sudo -u postgres psql << EOF
CREATE DATABASE linkinbio;
CREATE USER linkinbio_user WITH ENCRYPTED PASSWORD 'your_password';
GRANT ALL PRIVILEGES ON DATABASE linkinbio TO linkinbio_user;
\q
EOF

# Применить миграции
psql -U linkinbio_user -d linkinbio -f LinkInBio.Backend/Database/Migrations.sql
```

### Настройка приложения

```bash
# 1. Клонировать репозиторий
cd /var/www
sudo git clone https://github.com/yourusername/LinkInBio.pro.git
cd LinkInBio.pro/LinkInBio.Backend

# 2. Собрать приложение
dotnet publish -c Release -o /var/www/linkinbio

# 3. Создать systemd service
sudo tee /etc/systemd/system/linkinbio.service > /dev/null << EOF
[Unit]
Description=LinkInBio.pro API
After=network.target

[Service]
WorkingDirectory=/var/www/linkinbio
ExecStart=/root/.dotnet/dotnet /var/www/linkinbio/LinkInBio.Backend.dll
Restart=always
RestartSec=10
SyslogIdentifier=linkinbio
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DATABASE_URL=Host=localhost;Database=linkinbio;Username=linkinbio_user;Password=your_password
Environment=JWT_SECRET=your_jwt_secret_here

[Install]
WantedBy=multi-user.target
EOF

# 4. Запустить сервис
sudo systemctl enable linkinbio
sudo systemctl start linkinbio
sudo systemctl status linkinbio
```

### Настройка Nginx

```bash
sudo tee /etc/nginx/sites-available/linkinbio << EOF
server {
    listen 80;
    server_name linkinbio.pro www.linkinbio.pro;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade \$http_upgrade;
        proxy_set_header Connection 'upgrade';
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
        proxy_cache_bypass \$http_upgrade;
    }
}
EOF

# Активировать конфигурацию
sudo ln -s /etc/nginx/sites-available/linkinbio /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl restart nginx
```

### SSL сертификат

```bash
# Получить Let's Encrypt сертификат
sudo certbot --nginx -d linkinbio.pro -d www.linkinbio.pro

# Автоматическое обновление
sudo certbot renew --dry-run
```

---

## 📊 Мониторинг

### Логи

```bash
# Docker
docker-compose logs -f backend

# Systemd
sudo journalctl -u linkinbio -f

# Nginx
sudo tail -f /var/log/nginx/access.log
sudo tail -f /var/log/nginx/error.log
```

### Health Check

```bash
# Проверка API
curl http://localhost:5000/health

# Проверка PostgreSQL
sudo -u postgres psql -c "SELECT version();"
```

---

## 🔒 Безопасность

### Checklist для Production

- [ ] Изменить JWT_SECRET на уникальный ключ
- [ ] Использовать сильные пароли для БД
- [ ] Настроить HTTPS (SSL/TLS)
- [ ] Включить CORS только для нужных доменов
- [ ] Настроить firewall (ufw/iptables)
- [ ] Регулярные бэкапы БД
- [ ] Настроить rate limiting
- [ ] Логирование и мониторинг
- [ ] Обновлять зависимости

### Firewall (UFW)

```bash
sudo ufw allow 22/tcp
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable
```

### Rate Limiting (Nginx)

Добавить в nginx.conf:

```nginx
limit_req_zone $binary_remote_addr zone=api_limit:10m rate=10r/s;

location /api/ {
    limit_req zone=api_limit burst=20 nodelay;
    # ... остальная конфигурация
}
```

---

## 💾 Бэкапы

### PostgreSQL Backup

```bash
# Создать бэкап
pg_dump -U linkinbio_user linkinbio > backup_$(date +%Y%m%d_%H%M%S).sql

# Восстановить из бэкапа
psql -U linkinbio_user linkinbio < backup_20240101_120000.sql
```

### Автоматические бэкапы (cron)

```bash
# Добавить в crontab
crontab -e

# Бэкап каждый день в 3:00
0 3 * * * pg_dump -U linkinbio_user linkinbio > /backups/db_$(date +\%Y\%m\%d).sql
```

---

## 🚀 CI/CD

### GitHub Actions

Создать `.github/workflows/deploy.yml`:

```yaml
name: Deploy

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Build
        run: |
          cd LinkInBio.Backend
          dotnet restore
          dotnet build -c Release
          dotnet test

      - name: Deploy to Railway
        run: |
          npm i -g @railway/cli
          railway up
        env:
          RAILWAY_TOKEN: ${{ secrets.RAILWAY_TOKEN }}
```

---

## 📈 Масштабирование

### Kubernetes

Пример `k8s-deployment.yaml`:

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: linkinbio-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: linkinbio-api
  template:
    metadata:
      labels:
        app: linkinbio-api
    spec:
      containers:
      - name: api
        image: yourusername/linkinbio-backend:latest
        ports:
        - containerPort: 5000
        env:
        - name: DATABASE_URL
          valueFrom:
            secretKeyRef:
              name: linkinbio-secrets
              key: database-url
        - name: JWT_SECRET
          valueFrom:
            secretKeyRef:
              name: linkinbio-secrets
              key: jwt-secret
---
apiVersion: v1
kind: Service
metadata:
  name: linkinbio-api
spec:
  type: LoadBalancer
  ports:
  - port: 80
    targetPort: 5000
  selector:
    app: linkinbio-api
```

Деплой:
```bash
kubectl apply -f k8s-deployment.yaml
```
