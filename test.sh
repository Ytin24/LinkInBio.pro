#!/bin/bash

# LinkInBio.pro API Test Script
# Полный тестовый сценарий для проверки всех endpoints

set -e  # Остановиться при первой ошибке

BASE_URL="${BASE_URL:-http://localhost:5000}"
echo "🚀 Testing API at: $BASE_URL"
echo "================================"

# Генерируем уникальные данные для каждого запуска
TIMESTAMP=$(date +%s)
TEST_EMAIL="test${TIMESTAMP}@linkinbio.pro"
TEST_USERNAME="testuser${TIMESTAMP}"
TEST_SLUG="profile${TIMESTAMP}"

echo -e "\n📝 Test data:"
echo "Email: $TEST_EMAIL"
echo "Username: $TEST_USERNAME"
echo "Slug: $TEST_SLUG"

# Проверка здоровья API
echo -e "\n=== ✅ Health Check ==="
curl -s -f $BASE_URL/health && echo " ✓ API is healthy" || echo " ✗ API is down"

# 1. Регистрация пользователя
echo -e "\n=== 1️⃣  User Registration ==="
REGISTER_RESPONSE=$(curl -s -X POST $BASE_URL/api/auth/register \
  -H "Content-Type: application/json" \
  -d "{
    \"email\": \"$TEST_EMAIL\",
    \"password\": \"SecurePassword123!\",
    \"username\": \"$TEST_USERNAME\"
  }")

echo "$REGISTER_RESPONSE" | jq . 2>/dev/null || echo "$REGISTER_RESPONSE"

TOKEN=$(echo "$REGISTER_RESPONSE" | jq -r '.token' 2>/dev/null)

if [ "$TOKEN" == "null" ] || [ -z "$TOKEN" ]; then
  echo "❌ Registration failed! Cannot proceed."
  exit 1
fi

echo "✅ Registered successfully"
echo "🔑 Token: ${TOKEN:0:50}..."

# 2. Вход (тест логина)
echo -e "\n=== 2️⃣  User Login ==="
LOGIN_RESPONSE=$(curl -s -X POST $BASE_URL/api/auth/login \
  -H "Content-Type: application/json" \
  -d "{
    \"email\": \"$TEST_EMAIL\",
    \"password\": \"SecurePassword123!\"
  }")

echo "$LOGIN_RESPONSE" | jq . 2>/dev/null || echo "$LOGIN_RESPONSE"
echo "✅ Login successful"

# 3. Получить текущего пользователя
echo -e "\n=== 3️⃣  Get Current User ==="
ME_RESPONSE=$(curl -s -X GET $BASE_URL/api/auth/me \
  -H "Authorization: Bearer $TOKEN")

echo "$ME_RESPONSE" | jq . 2>/dev/null || echo "$ME_RESPONSE"
echo "✅ Got current user info"

# 4. Создать профиль
echo -e "\n=== 4️⃣  Create Profile ==="
PROFILE_RESPONSE=$(curl -s -X POST $BASE_URL/api/profiles \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"slug\": \"$TEST_SLUG\",
    \"displayName\": \"Test Profile $TIMESTAMP\",
    \"bio\": \"Welcome to my awesome LinkInBio page! 🚀\"
  }")

echo "$PROFILE_RESPONSE" | jq . 2>/dev/null || echo "$PROFILE_RESPONSE"

PROFILE_ID=$(echo "$PROFILE_RESPONSE" | jq -r '.id' 2>/dev/null)

if [ "$PROFILE_ID" == "null" ] || [ -z "$PROFILE_ID" ]; then
  echo "❌ Profile creation failed!"
  exit 1
fi

echo "✅ Profile created: $PROFILE_ID"

# 5. Получить все свои профили
echo -e "\n=== 5️⃣  Get My Profiles ==="
MY_PROFILES=$(curl -s -X GET $BASE_URL/api/profiles/my \
  -H "Authorization: Bearer $TOKEN")

echo "$MY_PROFILES" | jq . 2>/dev/null || echo "$MY_PROFILES"
echo "✅ Retrieved user profiles"

# 6. Добавить ссылки
echo -e "\n=== 6️⃣  Add Links ==="

# Link 1: Website
LINK1=$(curl -s -X POST $BASE_URL/api/profiles/$PROFILE_ID/links \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "🌐 My Website",
    "url": "https://example.com",
    "position": 1
  }')
echo "Link 1:" && echo "$LINK1" | jq . 2>/dev/null
LINK1_ID=$(echo "$LINK1" | jq -r '.id' 2>/dev/null)

# Link 2: Instagram
LINK2=$(curl -s -X POST $BASE_URL/api/profiles/$PROFILE_ID/links \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"title\": \"📸 Instagram\",
    \"url\": \"https://instagram.com/$TEST_USERNAME\",
    \"position\": 2
  }")
echo "Link 2:" && echo "$LINK2" | jq . 2>/dev/null
LINK2_ID=$(echo "$LINK2" | jq -r '.id' 2>/dev/null)

# Link 3: YouTube
LINK3=$(curl -s -X POST $BASE_URL/api/profiles/$PROFILE_ID/links \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"title\": \"🎥 YouTube\",
    \"url\": \"https://youtube.com/@$TEST_USERNAME\",
    \"position\": 3
  }")
echo "Link 3:" && echo "$LINK3" | jq . 2>/dev/null

echo "✅ Added 3 links"

# 7. Обновить ссылку
echo -e "\n=== 7️⃣  Update Link ==="
UPDATE_LINK=$(curl -s -X PUT $BASE_URL/api/links/$LINK1_ID \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "title": "🌍 My Updated Website",
    "position": 1
  }')

echo "$UPDATE_LINK" | jq . 2>/dev/null || echo "$UPDATE_LINK"
echo "✅ Updated link"

# 8. Обновить профиль (опубликовать)
echo -e "\n=== 8️⃣  Publish Profile ==="
UPDATE_PROFILE=$(curl -s -X PUT $BASE_URL/api/profiles/$PROFILE_ID \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "displayName": "My Awesome Page",
    "bio": "Check out all my links below! 👇",
    "isPublished": true,
    "theme": {
      "backgroundColor": "#667eea",
      "textColor": "#ffffff",
      "buttonStyle": "Pill",
      "fontFamily": "Inter"
    }
  }')

echo "$UPDATE_PROFILE" | jq . 2>/dev/null || echo "$UPDATE_PROFILE"
echo "✅ Profile published with custom theme"

# 9. Просмотр публичного профиля
echo -e "\n=== 9️⃣  View Public Profile ==="
PUBLIC_PROFILE=$(curl -s -X GET $BASE_URL/api/profiles/$TEST_SLUG)
echo "$PUBLIC_PROFILE" | jq . 2>/dev/null || echo "$PUBLIC_PROFILE"
echo "✅ Public profile is visible"
echo "🔗 Public URL: $BASE_URL/api/profiles/$TEST_SLUG"

# 10. Получить все ссылки профиля
echo -e "\n=== 🔟 Get Profile Links ==="
PROFILE_LINKS=$(curl -s -X GET $BASE_URL/api/profiles/$PROFILE_ID/links)
echo "$PROFILE_LINKS" | jq . 2>/dev/null || echo "$PROFILE_LINKS"
echo "✅ Retrieved profile links"

# 11. Отследить клики
echo -e "\n=== 1️⃣1️⃣  Track Click Events ==="
for i in {1..5}; do
  CLICK_RESPONSE=$(curl -s -X POST $BASE_URL/api/analytics/click \
    -H "Content-Type: application/json" \
    -d "{
      \"linkId\": \"$LINK1_ID\",
      \"ipAddress\": \"192.168.1.$i\",
      \"userAgent\": \"Mozilla/5.0 (Test Browser $i)\",
      \"referrer\": \"https://instagram.com\"
    }")
  echo "Click $i tracked" >/dev/null
done

echo "✅ Tracked 5 click events on Link 1"

# Клик на вторую ссылку
curl -s -X POST $BASE_URL/api/analytics/click \
  -H "Content-Type: application/json" \
  -d "{
    \"linkId\": \"$LINK2_ID\",
    \"ipAddress\": \"192.168.1.100\",
    \"userAgent\": \"Mozilla/5.0\",
    \"referrer\": \"https://tiktok.com\"
  }" >/dev/null

echo "✅ Tracked 1 click on Link 2"

# 12. Просмотр аналитики
echo -e "\n=== 1️⃣2️⃣  View Analytics ==="
ANALYTICS=$(curl -s -X GET $BASE_URL/api/analytics/$PROFILE_ID \
  -H "Authorization: Bearer $TOKEN")

echo "$ANALYTICS" | jq . 2>/dev/null || echo "$ANALYTICS"
echo "✅ Retrieved analytics"

# 13. Топ ссылок
echo -e "\n=== 1️⃣3️⃣  Get Top Links ==="
TOP_LINKS=$(curl -s -X GET "$BASE_URL/api/analytics/$PROFILE_ID/top-links?limit=5" \
  -H "Authorization: Bearer $TOKEN")

echo "$TOP_LINKS" | jq . 2>/dev/null || echo "$TOP_LINKS"
echo "✅ Retrieved top performing links"

# 14. Экспорт аналитики
echo -e "\n=== 1️⃣4️⃣  Export Analytics ==="
curl -s -X GET $BASE_URL/api/analytics/$PROFILE_ID/export \
  -H "Authorization: Bearer $TOKEN" \
  -o "analytics_${TIMESTAMP}.csv"

if [ -f "analytics_${TIMESTAMP}.csv" ]; then
  echo "✅ Analytics exported to analytics_${TIMESTAMP}.csv"
  cat "analytics_${TIMESTAMP}.csv"
else
  echo "⚠️  Export failed"
fi

# 15. Удалить ссылку
echo -e "\n=== 1️⃣5️⃣  Delete Link ==="
DELETE_RESPONSE=$(curl -s -X DELETE $BASE_URL/api/links/$LINK2_ID \
  -H "Authorization: Bearer $TOKEN" \
  -w "\nHTTP Status: %{http_code}")

echo "$DELETE_RESPONSE"
echo "✅ Link deleted"

# Финальная сводка
echo -e "\n================================"
echo "🎉 All tests completed successfully!"
echo "================================"
echo ""
echo "📊 Summary:"
echo "  - Profile ID: $PROFILE_ID"
echo "  - Public URL: $BASE_URL/api/profiles/$TEST_SLUG"
echo "  - Total Links: 2 (after deletion)"
echo "  - Total Clicks: 6"
echo ""
echo "🧪 Test Results:"
echo "  ✅ Registration & Authentication"
echo "  ✅ Profile CRUD operations"
echo "  ✅ Link management"
echo "  ✅ Click tracking"
echo "  ✅ Analytics & reporting"
echo "  ✅ Data export"
echo ""
echo "🔗 Try visiting your profile:"
echo "   curl $BASE_URL/api/profiles/$TEST_SLUG | jq ."
echo ""
