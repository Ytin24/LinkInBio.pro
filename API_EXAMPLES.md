# LinkInBio.pro - API Examples

Примеры запросов для тестирования API.

## 🔐 Аутентификация

### 1. Регистрация нового пользователя

```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "password123",
    "username": "testuser"
  }'
```

**Ответ:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "username": "testuser",
  "email": "test@example.com"
}
```

### 2. Вход

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "password123"
  }'
```

### 3. Получить текущего пользователя

```bash
export TOKEN="your-jwt-token-here"

curl -X GET http://localhost:5000/api/auth/me \
  -H "Authorization: Bearer $TOKEN"
```

---

## 👤 Профили

### 4. Создать новый профиль

```bash
curl -X POST http://localhost:5000/api/profiles \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "slug": "myawesomepage",
    "displayName": "My Awesome Page",
    "bio": "Welcome to my links! 🚀"
  }'
```

### 5. Получить профиль (публичный)

```bash
curl -X GET http://localhost:5000/api/profiles/myawesomepage
```

### 6. Получить все свои профили

```bash
curl -X GET http://localhost:5000/api/profiles/my \
  -H "Authorization: Bearer $TOKEN"
```

### 7. Обновить профиль

```bash
export PROFILE_ID="profile-uuid-here"

curl -X PUT http://localhost:5000/api/profiles/$PROFILE_ID \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "displayName": "Updated Name",
    "bio": "New bio text",
    "isPublished": true,
    "theme": {
      "backgroundColor": "#1a1a1a",
      "textColor": "#ffffff",
      "buttonStyle": "Pill",
      "fontFamily": "Inter"
    }
  }'
```

---

## 🔗 Ссылки

### 8. Добавить ссылку в профиль

```bash
export PROFILE_ID="profile-uuid-here"

curl -X POST http://localhost:5000/api/profiles/$PROFILE_ID/links \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "My Instagram",
    "url": "https://instagram.com/myusername",
    "iconUrl": "https://cdn.example.com/instagram-icon.png",
    "position": 1
  }'
```

### 9. Получить все ссылки профиля

```bash
curl -X GET http://localhost:5000/api/profiles/$PROFILE_ID/links
```

### 10. Обновить ссылку

```bash
export LINK_ID="link-uuid-here"

curl -X PUT http://localhost:5000/api/links/$LINK_ID \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Updated Instagram",
    "url": "https://instagram.com/newtusername",
    "position": 2,
    "isActive": true
  }'
```

### 11. Удалить ссылку

```bash
curl -X DELETE http://localhost:5000/api/links/$LINK_ID \
  -H "Authorization: Bearer $TOKEN"
```

---

## 📊 Аналитика

### 12. Отследить клик по ссылке (публичный endpoint)

```bash
export LINK_ID="link-uuid-here"

curl -X POST http://localhost:5000/api/analytics/click \
  -H "Content-Type: application/json" \
  -d '{
    "linkId": "'$LINK_ID'",
    "ipAddress": "192.168.1.1",
    "userAgent": "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36",
    "referrer": "https://instagram.com"
  }'
```

### 13. Получить аналитику профиля

```bash
export PROFILE_ID="profile-uuid-here"

# Без фильтра по датам
curl -X GET http://localhost:5000/api/analytics/$PROFILE_ID \
  -H "Authorization: Bearer $TOKEN"

# С фильтром по датам
curl -X GET "http://localhost:5000/api/analytics/$PROFILE_ID?startDate=2024-01-01&endDate=2024-12-31" \
  -H "Authorization: Bearer $TOKEN"
```

### 14. Получить топ ссылок

```bash
curl -X GET "http://localhost:5000/api/analytics/$PROFILE_ID/top-links?limit=5" \
  -H "Authorization: Bearer $TOKEN"
```

### 15. Экспорт аналитики в CSV

```bash
curl -X GET http://localhost:5000/api/analytics/$PROFILE_ID/export \
  -H "Authorization: Bearer $TOKEN" \
  -o analytics.csv
```

---

## 🧪 Полный тестовый сценарий

```bash
#!/bin/bash

BASE_URL="http://localhost:5000"

echo "=== 1. Регистрация пользователя ==="
REGISTER_RESPONSE=$(curl -s -X POST $BASE_URL/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "demo@linkinbio.pro",
    "password": "SecurePass123!",
    "username": "demouser"
  }')

echo $REGISTER_RESPONSE | jq .

TOKEN=$(echo $REGISTER_RESPONSE | jq -r '.token')
echo "Token: $TOKEN"

echo -e "\n=== 2. Создание профиля ==="
PROFILE_RESPONSE=$(curl -s -X POST $BASE_URL/api/profiles \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "slug": "demouser",
    "displayName": "Demo User",
    "bio": "Check out my links! 🔥"
  }')

echo $PROFILE_RESPONSE | jq .
PROFILE_ID=$(echo $PROFILE_RESPONSE | jq -r '.id')
echo "Profile ID: $PROFILE_ID"

echo -e "\n=== 3. Добавление ссылок ==="
LINK1=$(curl -s -X POST $BASE_URL/api/profiles/$PROFILE_ID/links \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "My Website",
    "url": "https://example.com",
    "position": 1
  }')
echo $LINK1 | jq .
LINK1_ID=$(echo $LINK1 | jq -r '.id')

LINK2=$(curl -s -X POST $BASE_URL/api/profiles/$PROFILE_ID/links \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Instagram",
    "url": "https://instagram.com/demouser",
    "position": 2
  }')
echo $LINK2 | jq .

echo -e "\n=== 4. Публикация профиля ==="
curl -s -X PUT $BASE_URL/api/profiles/$PROFILE_ID \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "isPublished": true
  }' | jq .

echo -e "\n=== 5. Просмотр публичного профиля ==="
curl -s -X GET $BASE_URL/api/profiles/demouser | jq .

echo -e "\n=== 6. Отслеживание клика ==="
curl -s -X POST $BASE_URL/api/analytics/click \
  -H "Content-Type: application/json" \
  -d '{
    "linkId": "'$LINK1_ID'",
    "ipAddress": "192.168.1.100",
    "userAgent": "Mozilla/5.0",
    "referrer": "https://instagram.com"
  }' | jq .

echo -e "\n=== 7. Просмотр аналитики ==="
curl -s -X GET $BASE_URL/api/analytics/$PROFILE_ID \
  -H "Authorization: Bearer $TOKEN" | jq .

echo -e "\n=== Тест завершен! ==="
echo "Профиль доступен по адресу: $BASE_URL/api/profiles/demouser"
```

Сохраните скрипт в файл `test.sh` и выполните:
```bash
chmod +x test.sh
./test.sh
```

---

## 🎨 Примеры тем оформления

### Темная тема
```json
{
  "theme": {
    "backgroundColor": "#1a1a1a",
    "textColor": "#ffffff",
    "buttonStyle": "Pill",
    "fontFamily": "Inter"
  }
}
```

### Светлая минималистичная
```json
{
  "theme": {
    "backgroundColor": "#ffffff",
    "textColor": "#333333",
    "buttonStyle": "Square",
    "fontFamily": "Roboto"
  }
}
```

### Градиент (синий)
```json
{
  "theme": {
    "backgroundColor": "#667eea",
    "textColor": "#ffffff",
    "buttonStyle": "Rounded",
    "fontFamily": "Poppins"
  }
}
```

### Розовая тема для инфлюенсеров
```json
{
  "theme": {
    "backgroundColor": "#ff6b9d",
    "textColor": "#ffffff",
    "buttonStyle": "Pill",
    "fontFamily": "Montserrat"
  }
}
```

---

## 🐛 Тестирование ошибок

### Регистрация с существующим email
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "password123",
    "username": "anotheruser"
  }'
# Ожидаемый ответ: 400 Bad Request - "Email already registered"
```

### Доступ без токена
```bash
curl -X GET http://localhost:5000/api/profiles/my
# Ожидаемый ответ: 401 Unauthorized
```

### Невалидный slug
```bash
curl -X POST http://localhost:5000/api/profiles \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "slug": "my profile with spaces!",
    "displayName": "Test"
  }'
# Ожидаемый ответ: 400 Bad Request - "Slug must contain only letters, numbers, and hyphens"
```

### Занятый slug
```bash
curl -X POST http://localhost:5000/api/profiles \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "slug": "testuser",
    "displayName": "Another Profile"
  }'
# Ожидаемый ответ: 409 Conflict - "Slug already taken"
```
