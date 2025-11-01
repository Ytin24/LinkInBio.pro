# Docker Build Fix - Пошаговое решение

## Проблема

```
RUN dotnet publish LinkInBio.Backend.fsproj -c Release -o out
exit code: 1
```

## Причина

Ошибка компиляции F# проекта из-за:
1. Неправильных ссылок между модулями
2. Попытки открыть несуществующий namespace `LinkInBio.Backend.Handlers`
3. Неправильных путей к `Database.DbContext.Users` вместо `Users`

## Решение (Временное - минимальная версия)

Создана минимальная рабочая версия для проверки Docker build:

### 1. Упрощенный `Program.fs`
- Только health check endpoint `/health`
- Нет зависимостей от Handlers/Services
- Минимальный набор imports

### 2. Упрощенный `.fsproj`
- Только Program.fs
- Только Giraffe package
- Удалены все зависимости от Models/Database/Services

###  3. Резервные копии
- `Program.fs.backup` - полная версия
- `LinkInBio.Backend.fsproj.backup` - полная версия

## Тестирование Docker Build

```bash
# Перейти в директорию бэкенда
cd LinkInBio.Backend

# Собрать Docker образ
docker build -t linkinbio-minimal .

# Если успешно - запустить
docker run -p 5000:5000 linkinbio-minimal

# Проверить
curl http://localhost:5000/health
# Ответ: OK
```

## Восстановление полной версии

После успешной сборки минимальной версии:

### Шаг 1: Исправить импорты в Services

**`Services/AuthService.fs`:**
```fsharp
open LinkInBio.Backend.Database.DbContext  // добавить эту строку

// Заменить все:
Database.DbContext.Users.findByEmail
// на:
Users.findByEmail
```

**`Services/AnalyticsService.fs`:**
```fsharp
open LinkInBio.Backend.Database.DbContext  // добавить
```

### Шаг 2: Исправить импорты в Handlers

**`Handlers/ProfileHandlers.fs`:**
```fsharp
open LinkInBio.Backend.Database.DbContext  // добавить
```

**`Handlers/LinkHandlers.fs`:**
```fsharp
open LinkInBio.Backend.Database.DbContext  // добавить
```

**`Handlers/AnalyticsHandlers.fs`:**
```fsharp
open LinkInBio.Backend.Database.DbContext  // добавить
open LinkInBio.Backend.Services  // добавить
```

### Шаг 3: Исправить Program.fs

Убрать эту строку:
```fsharp
open LinkInBio.Backend.Handlers  // ❌ УДАЛИТЬ
```

Использовать полные имена модулей:
```fsharp
// ✅ ПРАВИЛЬНО:
POST >=> route "/auth/register" >=> AuthHandlers.register
```

### Шаг 4: Обновить .fsproj

Вернуть все файлы в правильном порядке:

```xml
<ItemGroup>
  <!-- Models - ПЕРВЫМИ -->
  <Compile Include="Models/Domain.fs" />
  <Compile Include="Models/DTOs.fs" />

  <!-- Database -->
  <Compile Include="Database/DbContext.fs" />

  <!-- Services - ПОСЛЕ Database -->
  <Compile Include="Services/AuthService.fs" />
  <Compile Include="Services/AnalyticsService.fs" />

  <!-- Handlers - ПОСЛЕ Services -->
  <Compile Include="Handlers/AuthHandlers.fs" />
  <Compile Include="Handlers/ProfileHandlers.fs" />
  <Compile Include="Handlers/LinkHandlers.fs" />
  <Compile Include="Handlers/AnalyticsHandlers.fs" />

  <!-- Program - ПОСЛЕДНИМ -->
  <Compile Include="Program.fs" />
</ItemGroup>
```

## Пошаговое добавление компонентов

### Этап 1: Модели + Database
```xml
<Compile Include="Models/Domain.fs" />
<Compile Include="Models/DTOs.fs" />
<Compile Include="Database/DbContext.fs" />
<Compile Include="Program.fs" />
```

Тест: `dotnet build` - должно собраться

### Этап 2: Добавить Services
```xml
<Compile Include="Services/AuthService.fs" />
<Compile Include="Services/AnalyticsService.fs" />
```

Тест: `dotnet build`

### Этап 3: Добавить Handlers
```xml
<Compile Include="Handlers/AuthHandlers.fs" />
<!-- ... остальные хендлеры -->
```

Тест: `dotnet build`

### Этап 4: Обновить Program.fs
Добавить routes, middleware, etc.

Тест: `dotnet build && dotnet run`

## Альтернативное решение (быстрое)

Если нужна полная версия сразу - использовать скрипт:

```bash
#!/bin/bash
# fix-all.sh

# Восстановить бэкапы
cp Program.fs.backup Program.fs
cp LinkInBio.Backend.fsproj.backup LinkInBio.Backend.fsproj

# Исправить Program.fs
sed -i '/open LinkInBio.Backend.Handlers/d' Program.fs

# Исправить все сервисы и хендлеры
for file in Services/*.fs Handlers/*.fs; do
    # Добавить импорт DbContext после объявления модуля
    sed -i '/^module /a\
\
open LinkInBio.Backend.Database.DbContext' "$file"
done

# Заменить Database.DbContext.Users на Users
sed -i 's/Database\.DbContext\.\(Users\|Profiles\|Links\|ClickEvents\)/\1/g' Services/*.fs Handlers/*.fs

echo "✅ All files fixed!"
```

## Проверка корректности

```bash
# 1. Проверка синтаксиса
dotnet build -v detailed

# 2. Если есть ошибки - смотрим детали
dotnet build -v diagnostic | grep error

# 3. Проверка конкретного файла
dotnet build /p:BuildProjectReferences=false
```

## Типичные ошибки F# и решения

### Ошибка: "The namespace or module 'X' is not defined"
**Решение:** Добавить `open` directive или проверить порядок файлов в .fsproj

### Ошибка: "The value or constructor 'X' is not defined"
**Решение:** Убедиться что модуль открыт через `open`

### Ошибка: "Files in libraries must begin with a namespace or module declaration"
**Решение:** Добавить `module ModuleName` в начало файла

### Ошибка: "A reference to the type 'X' is required"
**Решение:** Добавить NuGet пакет в .fsproj

## Текущий статус

✅ Минимальная версия создана
⏳ Docker build тестируется
⏳ Полная версия требует исправления импортов

## Следующие шаги

1. Протестировать Docker build минимальной версии
2. Если успешно - постепенно добавлять компоненты
3. Исправить все импорты в Services/Handlers
4. Собрать полную версию
5. Запустить тесты (test.sh)

## Полезные команды

```bash
# Локальная сборка (без Docker)
cd LinkInBio.Backend
dotnet restore
dotnet build
dotnet run

# Docker сборка
docker build -t linkinbio-backend .

# Docker с выводом ошибок
docker build --progress=plain --no-cache -t linkinbio-backend .

# Запуск
docker run -p 5000:5000 -e DATABASE_URL="..." linkinbio-backend
```

## Контакты

При проблемах с билдом - создать issue с выводом:
```bash
dotnet build -v detailed > build.log 2>&1
```
