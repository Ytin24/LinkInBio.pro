# LinkInBio.pro - Альтернатива Linktree

**Одна ссылка → множество ссылок** для Instagram, TikTok и других соцсетей.

## 🎯 Особенности

- **F# Backend** с Giraffe (ASP.NET Core)
- **PostgreSQL** база данных
- **JWT аутентификация**
- **Аналитика кликов** - отслеживание каждого клика
- **A/B тестирование** страниц (в разработке)
- **REST API** для интеграции
- **Кастомизация** - темы, цвета, стили кнопок

## 🚀 Быстрый старт

### Требования

- .NET 8.0 SDK
- PostgreSQL 15+
- Docker (опционально)

### Запуск с Docker

```bash
# Клонировать репозиторий
git clone https://github.com/yourusername/LinkInBio.pro.git
cd LinkInBio.pro

# Запустить через Docker Compose
docker-compose up -d

# API будет доступен на http://localhost:5000
```

### Локальный запуск

```bash
# 1. Установить PostgreSQL и создать базу данных
createdb linkinbio

# 2. Применить миграции
psql -d linkinbio -f LinkInBio.Backend/Database/Migrations.sql

# 3. Настроить переменные окружения
cp .env.example .env
# Отредактировать .env с вашими настройками

# 4. Запустить backend
cd LinkInBio.Backend
dotnet restore
dotnet run

# API запущен на http://localhost:5000
```

## 📚 API Документация

### Аутентификация

#### Регистрация
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123",
  "username": "myusername"
}
```

**Ответ:**
```json
{
  "token": "eyJhbGc...",
  "userId": "123e4567-e89b-12d3-a456-426614174000",
  "username": "myusername",
  "email": "user@example.com"
}
```

#### Вход
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

#### Получить текущего пользователя
```http
GET /api/auth/me
Authorization: Bearer <token>
```

---

### Профили

#### Создать профиль
```http
POST /api/profiles
Authorization: Bearer <token>
Content-Type: application/json

{
  "slug": "myprofile",
  "displayName": "My Awesome Profile",
  "bio": "Welcome to my links!"
}
```

#### Получить профиль по slug (публичный)
```http
GET /api/profiles/myprofile
```

**Ответ:**
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "slug": "myprofile",
  "displayName": "My Awesome Profile",
  "bio": "Welcome to my links!",
  "avatarUrl": null,
  "theme": {
    "backgroundColor": "#ffffff",
    "textColor": "#000000",
    "buttonStyle": "Rounded",
    "fontFamily": "Inter"
  },
  "links": [
    {
      "id": "...",
      "title": "My Website",
      "url": "https://example.com",
      "iconUrl": null,
      "position": 1,
      "isActive": true
    }
  ],
  "isPublished": true
}
```

#### Получить все свои профили
```http
GET /api/profiles/my
Authorization: Bearer <token>
```

#### Обновить профиль
```http
PUT /api/profiles/{profileId}
Authorization: Bearer <token>
Content-Type: application/json

{
  "displayName": "New Name",
  "bio": "Updated bio",
  "isPublished": true,
  "theme": {
    "backgroundColor": "#1a1a1a",
    "textColor": "#ffffff",
    "buttonStyle": "Pill",
    "fontFamily": "Roboto"
  }
}
```

---

### Ссылки

#### Добавить ссылку
```http
POST /api/profiles/{profileId}/links
Authorization: Bearer <token>
Content-Type: application/json

{
  "title": "My Instagram",
  "url": "https://instagram.com/myusername",
  "iconUrl": "https://example.com/icon.png",
  "position": 1
}
```

#### Получить все ссылки профиля
```http
GET /api/profiles/{profileId}/links
```

#### Обновить ссылку
```http
PUT /api/links/{linkId}
Authorization: Bearer <token>
Content-Type: application/json

{
  "title": "Updated Title",
  "url": "https://newurl.com",
  "position": 2,
  "isActive": true
}
```

#### Удалить ссылку
```http
DELETE /api/links/{linkId}
Authorization: Bearer <token>
```

---

### Аналитика

#### Отследить клик (публичный endpoint)
```http
POST /api/analytics/click
Content-Type: application/json

{
  "linkId": "123e4567-e89b-12d3-a456-426614174000",
  "ipAddress": "192.168.1.1",
  "userAgent": "Mozilla/5.0...",
  "referrer": "https://instagram.com"
}
```

#### Получить аналитику профиля
```http
GET /api/analytics/{profileId}?startDate=2024-01-01&endDate=2024-12-31
Authorization: Bearer <token>
```

**Ответ:**
```json
{
  "totalViews": 1000,
  "totalClicks": 450,
  "clicksByLink": {
    "link-id-1": 200,
    "link-id-2": 150,
    "link-id-3": 100
  },
  "clicksByDate": {},
  "topCountries": []
}
```

#### Экспорт аналитики в CSV
```http
GET /api/analytics/{profileId}/export
Authorization: Bearer <token>
```

#### Топ ссылок
```http
GET /api/analytics/{profileId}/top-links?limit=10
Authorization: Bearer <token>
```

---

## 🗄️ Схема базы данных

```
users
├── id (UUID, PK)
├── email (VARCHAR, UNIQUE)
├── password_hash (VARCHAR)
├── username (VARCHAR, UNIQUE)
├── created_at (TIMESTAMP)
└── subscription_tier (VARCHAR)

profiles
├── id (UUID, PK)
├── user_id (UUID, FK)
├── slug (VARCHAR, UNIQUE)
├── display_name (VARCHAR)
├── bio (TEXT)
├── avatar_url (TEXT)
├── theme (JSONB)
├── is_published (BOOLEAN)
├── created_at (TIMESTAMP)
└── updated_at (TIMESTAMP)

links
├── id (UUID, PK)
├── profile_id (UUID, FK)
├── title (VARCHAR)
├── url (TEXT)
├── icon_url (TEXT)
├── position (INTEGER)
├── is_active (BOOLEAN)
└── created_at (TIMESTAMP)

click_events
├── id (UUID, PK)
├── link_id (UUID, FK)
├── profile_id (UUID, FK)
├── clicked_at (TIMESTAMP)
├── ip_address (VARCHAR)
├── user_agent (TEXT)
├── referrer (TEXT)
├── country (VARCHAR)
└── city (VARCHAR)
```

## 💰 Монетизация

### Тарифные планы

**Free (бесплатно)**
- 1 профиль
- До 10 ссылок
- Базовая аналитика
- Стандартные темы

**Basic (500₽/месяц)**
- 3 профиля
- Неограниченные ссылки
- Расширенная аналитика
- Кастомные темы
- Удаление брендинга

**Premium (1500₽/месяц)**
- Неограниченные профили
- Приоритетная поддержка
- A/B тестирование
- Экспорт данных
- Кастомный домен
- API доступ

## 🎨 Темы оформления

Каждый профиль можно кастомизировать:

```fsharp
type Theme = {
    BackgroundColor: string    // "#ffffff"
    TextColor: string          // "#000000"
    ButtonStyle: ButtonStyle   // Rounded | Square | Pill
    FontFamily: string         // "Inter", "Roboto", etc.
}
```

## 📊 A/B тестирование (в разработке)

Функционал для тестирования двух вариантов оформления профиля:
- Автоматическое распределение трафика 50/50
- Отслеживание конверсий
- Статистическая значимость результатов
- Определение победителя

## 🔐 Безопасность

- ✅ JWT токены с истечением
- ✅ BCrypt хеширование паролей (11 раундов)
- ✅ Валидация всех входных данных
- ✅ CORS настроен (нужно обновить для production)
- ⚠️ HTTPS обязателен в production
- ⚠️ Настроить rate limiting для API

## 🚧 Roadmap

- [ ] A/B тестирование UI
- [ ] Geo-IP определение локации
- [ ] Интеграция с платежными системами (Stripe, ЮKassa)
- [ ] Email уведомления
- [ ] Кастомные домены
- [ ] Расширенная аналитика (UTM параметры)
- [ ] Экспорт в различные форматы (JSON, Excel)
- [ ] Admin панель
- [ ] Frontend на Next.js
- [ ] Mobile приложение

## 📝 Лицензия

MIT License

## 🤝 Контрибьюция

Pull requests приветствуются! Для больших изменений сначала откройте issue.

## 📧 Контакты

- Email: support@linkinbio.pro
- GitHub: https://github.com/yourusername/LinkInBio.pro

---

Made with ❤️ and F#
