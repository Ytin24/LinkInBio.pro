# Docker Setup для LinkInBio.pro

Полное руководство по запуску проекта через Docker.

## 📋 Требования

- Docker Engine 20.10+
- Docker Compose 2.0+ (или docker-compose 1.29+)
- 2GB свободной оперативной памяти
- 5GB свободного места на диске

## 🚀 Быстрый старт

### 1. Production запуск

```bash
# Клонировать репозиторий
git clone <repository-url>
cd LinkInBio.pro

# Запустить все сервисы (PostgreSQL + Backend)
docker-compose up -d

# Проверить статус
docker-compose ps

# Посмотреть логи
docker-compose logs -f backend
```

API будет доступен на `http://localhost:5000`

### 2. Development запуск (с hot reload)

```bash
# Запустить в dev режиме
docker-compose --profile dev up -d backend-dev

# Теперь API доступен на порту 5001 с автоперезагрузкой
# При изменении .fs файлов приложение автоматически перезапустится
```

## 📁 Структура Docker

### Dockerfiles

**`Dockerfile`** - Production образ (multi-stage build):
- Stage 1: Build - собирает приложение
- Stage 2: Runtime - минимальный runtime-образ
- Использует non-root пользователя для безопасности
- Включает health check

**`Dockerfile.dev`** - Development образ:
- Один stage с .NET SDK
- Hot reload через `dotnet watch`
- Volume mapping для изменений в реальном времени

**`.dockerignore`**:
- Исключает bin/, obj/, .git/ из контекста сборки
- Ускоряет сборку образа

### Docker Compose

**Production сервисы:**

1. **postgres** - PostgreSQL 15
   - Порт: 5432
   - Auto-init с migrations.sql
   - Health check каждые 10 секунд
   - Persistent volume для данных

2. **backend** - F# API
   - Порт: 5000
   - Зависит от postgres
   - JWT authentication
   - Health check endpoint `/health`

**Development сервисы:**

3. **backend-dev** (profile: dev)
   - Порт: 5001
   - Volume mapping для hot reload
   - Development environment
   - Запускается через `dotnet watch`

## 🛠️ Команды Docker

### Управление контейнерами

```bash
# Запустить все сервисы
docker-compose up -d

# Запустить только PostgreSQL
docker-compose up -d postgres

# Запустить в dev режиме
docker-compose --profile dev up -d

# Остановить все сервисы
docker-compose down

# Остановить и удалить volumes (⚠️ удалит БД!)
docker-compose down -v

# Перезапустить сервис
docker-compose restart backend

# Посмотреть статус
docker-compose ps
```

### Логи

```bash
# Все логи
docker-compose logs

# Логи конкретного сервиса
docker-compose logs backend

# Следить за логами в реальном времени
docker-compose logs -f backend

# Последние 100 строк
docker-compose logs --tail=100 backend
```

### Сборка образов

```bash
# Пересобрать все образы
docker-compose build

# Пересобрать без кэша
docker-compose build --no-cache

# Пересобрать конкретный сервис
docker-compose build backend

# Собрать и запустить
docker-compose up -d --build
```

### Выполнение команд

```bash
# Bash в контейнере backend
docker-compose exec backend bash

# Посмотреть переменные окружения
docker-compose exec backend env

# PostgreSQL CLI
docker-compose exec postgres psql -U postgres -d linkinbio

# Выполнить SQL скрипт
docker-compose exec -T postgres psql -U postgres -d linkinbio < script.sql
```

## 🗄️ База данных

### Подключение к PostgreSQL

```bash
# Через docker-compose
docker-compose exec postgres psql -U postgres -d linkinbio

# Напрямую (если PostgreSQL запущен)
psql -h localhost -U postgres -d linkinbio
```

### Миграции

```bash
# Миграции применяются автоматически при первом запуске
# Если нужно применить вручную:

docker-compose exec postgres psql -U postgres -d linkinbio -f /docker-entrypoint-initdb.d/init.sql

# Или с хоста:
docker-compose exec -T postgres psql -U postgres -d linkinbio < LinkInBio.Backend/Database/Migrations.sql
```

### Бэкапы

```bash
# Создать бэкап
docker-compose exec postgres pg_dump -U postgres linkinbio > backup.sql

# Восстановить из бэкапа
docker-compose exec -T postgres psql -U postgres linkinbio < backup.sql

# Автоматический бэкап (добавить в cron)
0 3 * * * cd /path/to/project && docker-compose exec postgres pg_dump -U postgres linkinbio > backups/db_$(date +\%Y\%m\%d).sql
```

## 🔧 Конфигурация

### Переменные окружения

Создайте `.env` файл в корне проекта:

```bash
# Database
POSTGRES_DB=linkinbio
POSTGRES_USER=postgres
POSTGRES_PASSWORD=your_secure_password

# Application
JWT_SECRET=your-super-secret-jwt-key-min-32-characters
ASPNETCORE_ENVIRONMENT=Production

# Ports
BACKEND_PORT=5000
POSTGRES_PORT=5432
```

Затем измените `docker-compose.yml`:

```yaml
services:
  postgres:
    environment:
      POSTGRES_DB: ${POSTGRES_DB}
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}

  backend:
    environment:
      DATABASE_URL: "Host=postgres;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}"
      JWT_SECRET: ${JWT_SECRET}
    ports:
      - "${BACKEND_PORT}:5000"
```

### Volumes

```bash
# Посмотреть volumes
docker volume ls

# Инспектировать volume
docker volume inspect linkinbiopro_postgres_data

# Удалить неиспользуемые volumes
docker volume prune
```

## 🐛 Отладка

### Проблема: Backend не запускается

```bash
# Проверить логи
docker-compose logs backend

# Убедиться что PostgreSQL запущен
docker-compose ps postgres

# Проверить health check
docker-compose exec postgres pg_isready -U postgres
```

### Проблема: Ошибки сборки

```bash
# Очистить кэш Docker
docker builder prune

# Пересобрать без кэша
docker-compose build --no-cache backend

# Проверить .dockerignore
cat LinkInBio.Backend/.dockerignore
```

### Проблема: Не можется подключиться к БД

```bash
# Проверить сеть
docker network inspect linkinbiopro_backend

# Проверить что контейнеры в одной сети
docker-compose ps

# Попробовать подключиться вручную
docker-compose exec backend bash
apt-get update && apt-get install -y postgresql-client
psql -h postgres -U postgres -d linkinbio
```

### Проблема: Health check fails

```bash
# Проверить health check
docker inspect --format='{{json .State.Health}}' linkinbio-backend

# Проверить endpoint вручную
docker-compose exec backend curl http://localhost:5000/health
```

## 📊 Мониторинг

### Health checks

```bash
# Проверить статус всех контейнеров
docker-compose ps

# Детальная информация о health
docker inspect linkinbio-backend | jq '.[0].State.Health'

# Автоматический мониторинг (требует curl)
watch -n 5 'curl -s http://localhost:5000/health'
```

### Метрики ресурсов

```bash
# Использование ресурсов
docker stats

# Только backend
docker stats linkinbio-backend

# В реальном времени
docker-compose top
```

## 🚀 Production Deployment

### С Nginx reverse proxy

Создайте `docker-compose.prod.yml`:

```yaml
version: '3.8'

services:
  nginx:
    image: nginx:alpine
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

  # Остальные сервисы...
```

Запуск:
```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### SSL/TLS

```bash
# Получить Let's Encrypt сертификат
docker run -it --rm \
  -v /etc/letsencrypt:/etc/letsencrypt \
  certbot/certbot certonly --standalone \
  -d linkinbio.pro -d www.linkinbio.pro
```

## 🧪 Тестирование

### Запуск тестов

```bash
# Запустить тесты внутри контейнера
docker-compose exec backend dotnet test

# Запустить test.sh
docker-compose exec backend bash -c "apt-get update && apt-get install -y curl jq && ./test.sh"

# Или с хоста (если curl установлен)
./test.sh
```

## 📝 Best Practices

1. **Не коммитить .env файлы** - используйте `.env.example` как шаблон
2. **Регулярно обновлять образы** - `docker-compose pull && docker-compose up -d`
3. **Мониторить логи** - настроить log rotation
4. **Бэкапы БД** - автоматизировать через cron
5. **Использовать secrets в production** - Docker Swarm secrets или Kubernetes secrets
6. **Ограничить ресурсы** - добавить `deploy.resources.limits` в compose file

## 🔗 Полезные ссылки

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Reference](https://docs.docker.com/compose/compose-file/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet)
- [PostgreSQL Docker](https://hub.docker.com/_/postgres)

## 🆘 Помощь

При проблемах создайте issue с:
- Выводом `docker-compose logs`
- Версией Docker: `docker --version`
- ОС и версией
