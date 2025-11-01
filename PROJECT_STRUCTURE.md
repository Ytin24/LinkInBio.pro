# Структура проекта LinkInBio.pro

```
LinkInBio.pro/
│
├── 📁 LinkInBio.Backend/           # F# Backend API
│   ├── 📁 Models/                  # Модели данных
│   │   ├── Domain.fs               # Доменные модели (User, Profile, Link, etc.)
│   │   └── DTOs.fs                 # Data Transfer Objects для API
│   │
│   ├── 📁 Database/                # База данных
│   │   ├── DbContext.fs            # Npgsql.FSharp операции с БД
│   │   └── Migrations.sql          # SQL миграции и схема БД
│   │
│   ├── 📁 Services/                # Бизнес-логика
│   │   ├── AuthService.fs          # Аутентификация (JWT, BCrypt)
│   │   └── AnalyticsService.fs     # Аналитика и статистика
│   │
│   ├── 📁 Handlers/                # API Handlers (endpoints)
│   │   ├── AuthHandlers.fs         # /api/auth/*
│   │   ├── ProfileHandlers.fs      # /api/profiles/*
│   │   ├── LinkHandlers.fs         # /api/links/*
│   │   └── AnalyticsHandlers.fs    # /api/analytics/*
│   │
│   ├── Program.fs                  # Entry point, маршруты, middleware
│   ├── appsettings.json            # Конфигурация приложения
│   ├── LinkInBio.Backend.fsproj    # F# project file
│   └── Dockerfile                  # Docker образ для backend
│
├── 📄 README.md                    # Главная документация
├── 📄 API_EXAMPLES.md              # Примеры curl запросов
├── 📄 DEPLOYMENT.md                # Инструкции по деплою
├── 📄 CONTRIBUTING.md              # Гайд для контрибьюторов
├── 📄 CHANGELOG.md                 # История изменений
├── 📄 LICENSE                      # MIT License
├── 📄 PROJECT_STRUCTURE.md         # Этот файл
│
├── 🐳 docker-compose.yml           # Docker Compose (PostgreSQL + API)
├── 🧪 test.sh                      # Скрипт для тестирования API
├── 📝 .env.example                 # Пример переменных окружения
└── 📝 .gitignore                   # Git ignore rules
```

## 📦 Компоненты проекта

### Backend (F# + Giraffe)

**Технологии:**
- F# 8.0
- Giraffe 6.4 (веб-фреймворк)
- Npgsql.FSharp 5.7 (PostgreSQL клиент)
- ASP.NET Core 8.0
- JWT Bearer Authentication
- BCrypt.Net для хеширования паролей

**Архитектура:**
```
Request → Giraffe Routes → Handlers → Services → Database → Response
```

### Models (Модели)

**Domain.fs** - Доменные модели:
- `User` - Пользователи системы
- `Profile` - Публичные профили (страницы с ссылками)
- `Link` - Ссылки в профиле
- `ClickEvent` - События кликов (аналитика)
- `ABTest` - A/B тесты (в разработке)
- `Theme` - Темы оформления

**DTOs.fs** - API контракты:
- Request DTOs (RegisterRequest, CreateProfileRequest, etc.)
- Response DTOs (AuthResponse, ProfileResponse, etc.)

### Database Layer

**DbContext.fs** - Модули для работы с БД:
- `Users` - CRUD операции с пользователями
- `Profiles` - Управление профилями
- `Links` - Управление ссылками
- `ClickEvents` - Трекинг кликов и аналитика

**Migrations.sql** - SQL схема:
- Создание таблиц
- Индексы для производительности
- Тестовые данные
- Комментарии к таблицам

### Services (Сервисы)

**AuthService.fs:**
- Регистрация пользователей
- Аутентификация (логин)
- JWT генерация и валидация
- BCrypt хеширование паролей

**AnalyticsService.fs:**
- Трекинг кликов
- Подсчет статистики
- Экспорт данных в CSV
- A/B тестирование (в разработке)

### Handlers (API Endpoints)

**AuthHandlers.fs:**
- `POST /api/auth/register` - Регистрация
- `POST /api/auth/login` - Вход
- `GET /api/auth/me` - Текущий пользователь

**ProfileHandlers.fs:**
- `POST /api/profiles` - Создать профиль
- `GET /api/profiles/:slug` - Публичный профиль
- `GET /api/profiles/my` - Мои профили
- `PUT /api/profiles/:id` - Обновить профиль

**LinkHandlers.fs:**
- `POST /api/profiles/:id/links` - Добавить ссылку
- `GET /api/profiles/:id/links` - Список ссылок
- `PUT /api/links/:id` - Обновить ссылку
- `DELETE /api/links/:id` - Удалить ссылку

**AnalyticsHandlers.fs:**
- `POST /api/analytics/click` - Отследить клик
- `GET /api/analytics/:id` - Получить аналитику
- `GET /api/analytics/:id/export` - Экспорт в CSV
- `GET /api/analytics/:id/top-links` - Топ ссылок

### Database Schema

**Основные таблицы:**

```sql
users
├── id (UUID, PK)
├── email (VARCHAR, UNIQUE)
├── password_hash (VARCHAR)
├── username (VARCHAR, UNIQUE)
├── subscription_tier (VARCHAR)
└── created_at (TIMESTAMP)

profiles
├── id (UUID, PK)
├── user_id (UUID, FK → users)
├── slug (VARCHAR, UNIQUE)
├── display_name (VARCHAR)
├── bio (TEXT)
├── avatar_url (TEXT)
├── theme (JSONB)
├── is_published (BOOLEAN)
└── created_at, updated_at

links
├── id (UUID, PK)
├── profile_id (UUID, FK → profiles)
├── title (VARCHAR)
├── url (TEXT)
├── position (INTEGER)
├── is_active (BOOLEAN)
└── created_at

click_events (Аналитика)
├── id (UUID, PK)
├── link_id (UUID, FK → links)
├── profile_id (UUID, FK → profiles)
├── clicked_at (TIMESTAMP)
├── ip_address (VARCHAR)
├── user_agent (TEXT)
├── referrer (TEXT)
└── country, city (VARCHAR)

ab_tests (A/B тестирование)
├── id (UUID, PK)
├── profile_id (UUID, FK → profiles)
├── name (VARCHAR)
├── variant_a_theme (JSONB)
├── variant_b_theme (JSONB)
├── is_active (BOOLEAN)
└── start_date, end_date
```

## 🔄 Поток данных

### Регистрация пользователя
```
1. POST /api/auth/register
2. AuthHandlers.register
3. AuthService.register
   ├── Валидация email
   ├── BCrypt.HashPassword
   └── Database.Users.create
4. AuthService.generateToken (JWT)
5. → AuthResponse (token, userId, username)
```

### Создание профиля
```
1. POST /api/profiles (+ JWT token)
2. ProfileHandlers.create
   ├── Валидация токена
   ├── Проверка slug уникальности
   └── Database.Profiles.create
3. → ProfileResponse
```

### Клик по ссылке
```
1. POST /api/analytics/click
2. AnalyticsHandlers.trackClick
3. AnalyticsService.trackClick
   └── Database.ClickEvents.create
4. → Success response
```

### Получение аналитики
```
1. GET /api/analytics/:id (+ JWT token)
2. AnalyticsHandlers.getAnalytics
3. AnalyticsService.getAnalytics
   ├── Database.ClickEvents.getAnalytics
   ├── Подсчет статистики
   └── Группировка данных
4. → AnalyticsResponse (views, clicks, breakdown)
```

## 🛠️ Разработка

### Добавление нового endpoint

1. **Создать DTO** в `Models/DTOs.fs`
```fsharp
type NewFeatureRequest = {
    Field1: string
    Field2: int
}
```

2. **Добавить DB операцию** в `Database/DbContext.fs`
```fsharp
module NewFeature =
    let create (item: NewFeature) = ...
```

3. **Реализовать Handler** в `Handlers/`
```fsharp
let createFeature : HttpHandler =
    fun next ctx -> ...
```

4. **Добавить маршрут** в `Program.fs`
```fsharp
POST >=> route "/api/features" >=> FeatureHandlers.create
```

### Добавление миграции БД

1. Обновить `Database/Migrations.sql`
2. Добавить новую таблицу/колонку
3. Обновить DbContext.fs
4. Обновить Domain модели

## 📊 Метрики проекта

**Файлов F#:** 11
**Строк кода:** ~2500+
**API Endpoints:** 15
**Таблиц БД:** 6
**Документации:** 7 файлов

## 🎯 Следующие шаги

1. **Тестирование** - добавить юнит-тесты
2. **Frontend** - Next.js приложение
3. **A/B тесты** - реализовать API
4. **Rate limiting** - защита от злоупотреблений
5. **Email уведомления** - SMTP интеграция

## 📚 Полезные ссылки

- [F# Documentation](https://docs.microsoft.com/en-us/dotnet/fsharp/)
- [Giraffe Documentation](https://github.com/giraffe-fsharp/Giraffe)
- [Npgsql.FSharp](https://github.com/Zaid-Ajaj/Npgsql.FSharp)
- [PostgreSQL Docs](https://www.postgresql.org/docs/)
