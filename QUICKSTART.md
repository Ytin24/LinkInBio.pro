# LinkInBio.pro - Quick Start

## 🚨 Текущий статус проекта

**Версия:** 0.1.0-minimal
**Статус:** Минимальная рабочая версия (Docker build исправлен)

### Что работает
✅ Docker сборка
✅ Минимальный API с health check
✅ PostgreSQL интеграция
✅ Базовая инфраструктура

### Что временно отключено
⏳ Аутентификация (JWT)
⏳ CRUD профилей
⏳ Управление ссылками
⏳ Аналитика кликов

**Причина:** Исправление ошибок компиляции F# модулей (см. BUILD_FIX.md)

---

## 🚀 Запуск (Минимальная версия)

### Вариант 1: Docker (Рекомендуется)

```bash
# 1. Клонировать репозиторий
git clone <repository-url>
cd LinkInBio.pro

# 2. Запустить PostgreSQL
docker-compose up -d postgres

# 3. Собрать и запустить backend
docker-compose up -d backend

# 4. Проверить
curl http://localhost:5000/health
# Ответ: OK

curl http://localhost:5000/
# Ответ: LinkInBio.pro API v0.1.0
```

### Вариант 2: Локально (.NET SDK required)

```bash
# 1. Установить .NET 8.0 SDK
# https://dotnet.microsoft.com/download/dotnet/8.0

# 2. Запустить PostgreSQL
docker-compose up -d postgres

# 3. Собрать и запустить
cd LinkInBio.Backend
dotnet restore
dotnet build
dotnet run

# 4. API доступен на http://localhost:5000
```

---

## 📊 Доступные эндпоинты (Минимальная версия)

| Метод | Путь | Описание |
|-------|------|----------|
| GET | `/health` | Health check (возвращает "OK") |
| GET | `/` | API info (версия) |

---

## 🔧 Восстановление полной версии

Полная версия с аутентификацией, профилями, ссылками и аналитикой доступна в бэкапах:

```bash
cd LinkInBio.Backend

# Восстановить полную версию
cp Program.fs.backup Program.fs
cp LinkInBio.Backend.fsproj.backup LinkInBio.Backend.fsproj

# Исправить импорты (автоматически)
bash ../fix-imports.sh

# Или вручную (см. BUILD_FIX.md)
```

**Затем:** Следуйте пошаговым инструкциям в [BUILD_FIX.md](BUILD_FIX.md)

---

## 📚 Полная документация

- **[README.md](README.md)** - Главная документация проекта
- **[BUILD_FIX.md](BUILD_FIX.md)** - Как восстановить полную версию
- **[API_EXAMPLES.md](API_EXAMPLES.md)** - Примеры API запросов (для полной версии)
- **[DOCKER.md](DOCKER.md)** - Docker руководство
- **[DEPLOYMENT.md](DEPLOYMENT.md)** - Деплой на production
- **[PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md)** - Архитектура проекта

---

## 🐛 Проблемы и решения

### "dotnet publish exit code 1"

**Решение:** Используйте текущую минимальную версию. Для восстановления полной версии см. BUILD_FIX.md

### PostgreSQL не запускается

```bash
# Проверить статус
docker-compose ps postgres

# Посмотреть логи
docker-compose logs postgres

# Пересоздать
docker-compose down -v
docker-compose up -d postgres
```

### Backend не подключается к БД

```bash
# Проверить переменные окружения
docker-compose exec backend env | grep DATABASE

# Проверить сеть
docker network inspect linkinbiopro_backend

# Проверить что PostgreSQL готов
docker-compose exec postgres pg_isready -U postgres
```

---

## 📈 Roadmap восстановления

### Этап 1: Модели и Database ✅
- [x] Domain models (User, Profile, Link, ClickEvent)
- [x] DTOs (Request/Response objects)
- [x] DbContext (PostgreSQL operations)

### Этап 2: Services ⏳
- [ ] Исправить AuthService imports
- [ ] Исправить AnalyticsService imports
- [ ] Протестировать компиляцию

### Этап 3: Handlers ⏳
- [ ] Исправить AuthHandlers imports
- [ ] Исправить ProfileHandlers imports
- [ ] Исправить LinkHandlers imports
- [ ] Исправить AnalyticsHandlers imports

### Этап 4: Program.fs ⏳
- [ ] Добавить middleware (CORS, JWT)
- [ ] Добавить все routes
- [ ] Добавить error handling

### Этап 5: Тестирование ⏳
- [ ] Юнит тесты
- [ ] Интеграционные тесты (test.sh)
- [ ] Docker build & run

---

## 🎯 Быстрый тест

```bash
#!/bin/bash

# Запустить все сервисы
docker-compose up -d

# Подождать 10 секунд
sleep 10

# Проверить health
curl -f http://localhost:5000/health && echo "✅ Backend OK" || echo "❌ Backend FAIL"

# Проверить PostgreSQL
docker-compose exec postgres pg_isready -U postgres && echo "✅ Database OK" || echo "❌ Database FAIL"

# Остановить
docker-compose down
```

Сохраните как `test-minimal.sh` и запустите: `bash test-minimal.sh`

---

## 💬 Поддержка

**Проблемы с Docker:** См. [DOCKER.md](DOCKER.md)
**Проблемы с сборкой:** См. [BUILD_FIX.md](BUILD_FIX.md)
**Вопросы по API:** См. [API_EXAMPLES.md](API_EXAMPLES.md)

**GitHub Issues:** [Create Issue](https://github.com/yourusername/LinkInBio.pro/issues)

---

## ⚡ Что дальше?

1. **Запустите минимальную версию** - убедитесь что Docker build работает
2. **Прочитайте BUILD_FIX.md** - понимание как устроен проект
3. **Восстановите функции пошагово** - Services → Handlers → Program
4. **Запустите тесты** - `./test.sh` (после восстановления)
5. **Деплой** - см. DEPLOYMENT.md

---

Made with ❤️ and F#
