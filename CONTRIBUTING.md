# Contributing to LinkInBio.pro

Спасибо за интерес к проекту! Мы приветствуем любой вклад в развитие LinkInBio.pro.

## 🤝 Как помочь проекту

### Способы внести вклад

1. **Сообщить об ошибке** - создать issue с описанием проблемы
2. **Предложить улучшение** - создать issue с предложением новой функции
3. **Исправить баг** - создать pull request с исправлением
4. **Добавить функционал** - реализовать новую фичу
5. **Улучшить документацию** - исправить опечатки, добавить примеры
6. **Написать тесты** - увеличить покрытие кода тестами

## 🚀 Начало работы

### 1. Форк репозитория

```bash
# Сделать форк через GitHub UI, затем клонировать
git clone https://github.com/YOUR_USERNAME/LinkInBio.pro.git
cd LinkInBio.pro

# Добавить upstream remote
git remote add upstream https://github.com/ORIGINAL_OWNER/LinkInBio.pro.git
```

### 2. Настройка окружения

```bash
# Установить зависимости (требуется .NET 8.0 SDK)
cd LinkInBio.Backend
dotnet restore

# Запустить PostgreSQL
docker-compose up -d postgres

# Применить миграции
psql -U postgres -h localhost -d linkinbio -f Database/Migrations.sql

# Запустить приложение
dotnet run
```

### 3. Создание ветки

```bash
# Создать ветку для вашей фичи/исправления
git checkout -b feature/your-feature-name
# или
git checkout -b fix/bug-description
```

## 📝 Стандарты кодирования

### F# Code Style

- Использовать **4 пробела** для отступов (не табы)
- Следовать [F# Style Guide](https://docs.microsoft.com/en-us/dotnet/fsharp/style-guide/)
- Использовать `camelCase` для локальных переменных
- Использовать `PascalCase` для типов и модулей
- Документировать публичные API с помощью XML-комментариев

### Пример хорошего кода

```fsharp
module MyModule

/// Описание функции
/// <param name="userId">ID пользователя</param>
/// <returns>Профиль пользователя или None</returns>
let getUserProfile (userId: Guid) : Async<Profile option> =
    async {
        let! profiles = Database.Profiles.findByUserId userId
        return List.tryHead profiles
    }
```

### Commit Messages

Используйте [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Типы:**
- `feat`: новая фича
- `fix`: исправление бага
- `docs`: изменения в документации
- `style`: форматирование кода
- `refactor`: рефакторинг без изменения функционала
- `test`: добавление тестов
- `chore`: обновление зависимостей, CI/CD

**Примеры:**

```bash
feat(auth): add JWT token refresh endpoint

Implemented token refresh functionality to allow users
to extend their session without re-authenticating.

Closes #123
```

```bash
fix(analytics): correct click count calculation

Fixed a bug where clicks were being counted twice
due to duplicate event listeners.

Fixes #456
```

```bash
docs(api): update authentication examples

Added curl examples for all authentication endpoints.
```

## 🧪 Тестирование

### Запуск тестов

```bash
# Юнит-тесты (когда будут добавлены)
dotnet test

# Интеграционные тесты через API
./test.sh
```

### Добавление тестов

При добавлении новых функций, пожалуйста:

1. Добавьте юнит-тесты для новой логики
2. Обновите интеграционные тесты в `test.sh`
3. Убедитесь, что все тесты проходят

## 📋 Pull Request Process

### Чеклист перед PR

- [ ] Код следует стандартам проекта
- [ ] Все тесты проходят
- [ ] Добавлена документация для новых функций
- [ ] Обновлен CHANGELOG.md (если применимо)
- [ ] Коммиты следуют Conventional Commits
- [ ] Нет конфликтов с main веткой

### Создание Pull Request

1. **Синхронизировать с upstream**
```bash
git fetch upstream
git rebase upstream/main
```

2. **Пушнуть изменения**
```bash
git push origin feature/your-feature-name
```

3. **Создать PR через GitHub UI**

В описании PR укажите:
- Что изменено и почему
- Ссылку на связанные issues (если есть)
- Скриншоты (для UI изменений)
- Инструкции по тестированию

### Шаблон PR

```markdown
## Описание
Краткое описание изменений.

## Тип изменений
- [ ] Bug fix (исправление бага)
- [ ] New feature (новая функция)
- [ ] Breaking change (ломающее изменение)
- [ ] Documentation update

## Связанные Issues
Closes #123

## Как протестировано
Опишите, как вы тестировали изменения.

## Скриншоты (если применимо)
Добавьте скриншоты для визуальных изменений.

## Чеклист
- [ ] Код следует стандартам проекта
- [ ] Добавлены/обновлены тесты
- [ ] Все тесты проходят
- [ ] Обновлена документация
```

## 🐛 Сообщения об ошибках

### Шаблон Bug Report

При создании issue об ошибке, пожалуйста, включите:

1. **Описание проблемы** - что пошло не так?
2. **Шаги для воспроизведения** - как повторить проблему?
3. **Ожидаемое поведение** - что должно было произойти?
4. **Актуальное поведение** - что произошло на самом деле?
5. **Окружение**:
   - ОС (Windows, macOS, Linux)
   - Версия .NET SDK
   - Версия PostgreSQL
   - Способ запуска (Docker, локально)
6. **Логи/скриншоты** - если есть

### Пример Issue

```markdown
## Описание
Не работает аутентификация после регистрации.

## Шаги для воспроизведения
1. POST /api/auth/register с валидными данными
2. Получить токен в ответе
3. Использовать токен для GET /api/auth/me
4. Получить 401 Unauthorized

## Ожидаемое поведение
Должен вернуться профиль пользователя.

## Актуальное поведение
Возвращается 401 Unauthorized.

## Окружение
- ОС: Ubuntu 22.04
- .NET SDK: 8.0.1
- PostgreSQL: 15.3
- Запуск: Docker Compose

## Логи
```
[ERROR] JWT validation failed: Invalid token
```

## 💡 Предложения улучшений

### Feature Request Template

```markdown
## Описание фичи
Краткое описание предлагаемой функции.

## Проблема
Какую проблему решает эта функция?

## Предлагаемое решение
Как должна работать эта функция?

## Альтернативы
Рассматривали ли вы альтернативные решения?

## Дополнительный контекст
Скриншоты, mockups, примеры из других проектов.
```

## 📚 Работа с документацией

### Где находится документация

- `README.md` - основная документация
- `API_EXAMPLES.md` - примеры использования API
- `DEPLOYMENT.md` - инструкции по деплою
- Комментарии в коде

### Как улучшить документацию

1. Исправить опечатки и грамматические ошибки
2. Добавить недостающие примеры
3. Улучшить существующие объяснения
4. Перевести на другие языки
5. Добавить диаграммы и схемы

## 🏗️ Архитектурные решения

При добавлении больших изменений:

1. Создайте issue для обсуждения
2. Опишите предлагаемую архитектуру
3. Получите feedback от maintainers
4. Только после согласования начинайте реализацию

## 🎯 Приоритеты проекта

### Высокий приоритет

- [ ] Юнит и интеграционные тесты
- [ ] A/B тестирование UI
- [ ] Rate limiting для API
- [ ] Geo-IP определение
- [ ] Email уведомления

### Средний приоритет

- [ ] Frontend на Next.js
- [ ] Платежная интеграция
- [ ] Admin панель
- [ ] Кастомные домены

### Низкий приоритет

- [ ] Mobile приложение
- [ ] Интеграция с соцсетями
- [ ] Webhook поддержка

## 👥 Maintainers

Текущие maintainers проекта:

- [@yourusername](https://github.com/yourusername)

## 📄 Лицензия

Внося свой вклад в проект, вы соглашаетесь с тем, что ваш код будет
лицензирован под MIT License.

## 🙏 Благодарности

Спасибо всем, кто вносит вклад в развитие LinkInBio.pro!

---

Если у вас есть вопросы, не стесняйтесь:
- Создать issue
- Написать в Discussions
- Связаться с maintainers напрямую
