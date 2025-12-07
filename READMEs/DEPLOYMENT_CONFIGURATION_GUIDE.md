# 🔧 Przewodnik Konfiguracji Produkcyjnej - Orbito

**Data:** 2025-11-28  
**Status:** Szablon utworzony - wymaga wypełnienia wartości produkcyjnych

---

## 📋 **PODSUMOWANIE**

Utworzono szablon `appsettings.Production.json` z wszystkimi wymaganymi sekcjami konfiguracyjnymi. **Przed wdrożeniem należy wypełnić wszystkie wartości oznaczone jako `YOUR_*` lub `CHANGE_THIS_*`**.

---

## ⚠️ **KRYTYCZNE WARTOŚCI DO ZMIANY**

### **1. Connection String (Baza Danych)**

**Lokalizacja:** `ConnectionStrings.DefaultConnection`

**Obecna wartość (szablon):**
```json
"DefaultConnection": "Server=YOUR_PRODUCTION_SERVER;Database=Orbito_Production;User Id=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=true;TrustServerCertificate=false;Encrypt=true"
```

**Co zmienić:**
- `YOUR_PRODUCTION_SERVER` → adres serwera SQL Server (np. `sqlserver.yourdomain.com` lub Azure SQL connection string)
- `YOUR_USER` → nazwa użytkownika bazy danych
- `YOUR_PASSWORD` → hasło użytkownika (⚠️ **NIE COMMITOWAĆ DO REPOZYTORIUM** - użyj environment variables lub Azure Key Vault)

**Rekomendacja:** Użyj **Azure Key Vault** lub **Environment Variables** dla hasła:
```bash
# Przykład z environment variable
export ConnectionStrings__DefaultConnection="Server=...;Database=...;User Id=...;Password=$DB_PASSWORD"
```

---

### **2. JWT Secret Key**

**Lokalizacja:** `Jwt.Key`

**Obecna wartość (szablon):**
```json
"Key": "CHANGE_THIS_TO_STRONG_SECRET_KEY_MINIMUM_32_CHARACTERS_IN_PRODUCTION"
```

**Co zmienić:**
- Wygeneruj **silny, losowy klucz** minimum 32 znaki
- **NIE UŻYWAJ** tego samego klucza co w development

**Jak wygenerować:**
```bash
# PowerShell
-join ((65..90) + (97..122) + (48..57) | Get-Random -Count 64 | ForEach-Object {[char]$_})

# Linux/Mac
openssl rand -base64 32
```

**⚠️ KRYTYCZNE:** 
- Klucz musi być **unikalny dla produkcji**
- **NIE COMMITOWAĆ** do repozytorium
- Przechowywać w **Azure Key Vault** lub **Environment Variables**

---

### **3. Stripe API Keys (Produkcyjne)**

**Lokalizacja:** `Stripe.SecretKey`, `Stripe.PublishableKey`, `Stripe.WebhookSecret`

**Obecna wartość (szablon):**
```json
"SecretKey": "sk_live_YOUR_PRODUCTION_SECRET_KEY",
"PublishableKey": "pk_live_YOUR_PRODUCTION_PUBLISHABLE_KEY",
"WebhookSecret": "whsec_YOUR_PRODUCTION_WEBHOOK_SECRET",
"Environment": "live"
```

**Co zmienić:**
- Pobierz **produkcyjne klucze** z Stripe Dashboard (https://dashboard.stripe.com/apikeys)
- Upewnij się, że używasz kluczy z prefiksem `sk_live_` i `pk_live_` (nie `sk_test_` lub `pk_test_`)
- Skonfiguruj **Webhook Secret** w Stripe Dashboard dla endpointu produkcyjnego

**⚠️ KRYTYCZNE:**
- **NIE COMMITOWAĆ** kluczy do repozytorium
- Użyj **Azure Key Vault** lub **Environment Variables**

---

### **4. CORS Origins (Produkcyjne Domeny)**

**Lokalizacja:** `Cors.AllowedOrigins`

**Obecna wartość (szablon):**
```json
"AllowedOrigins": [
  "https://yourdomain.com",
  "https://www.yourdomain.com",
  "https://app.yourdomain.com"
]
```

**Co zmienić:**
- Zastąp `yourdomain.com` rzeczywistą domeną produkcyjną
- Dodaj wszystkie domeny, z których frontend będzie korzystał
- **Używaj tylko HTTPS** w produkcji

**Przykład:**
```json
"AllowedOrigins": [
  "https://orbito.com",
  "https://www.orbito.com",
  "https://app.orbito.com"
]
```

---

### **5. JWT Issuer i Audience**

**Lokalizacja:** `Jwt.Issuer`, `Jwt.Audience`

**Obecna wartość (szablon):**
```json
"Issuer": "https://api.yourdomain.com",
"Audience": "https://yourdomain.com"
```

**Co zmienić:**
- `Issuer` → URL API produkcyjnego (np. `https://api.orbito.com`)
- `Audience` → URL frontendu produkcyjnego (np. `https://orbito.com`)

---

### **6. Redis Connection String**

**Lokalizacja:** `IdempotencySettings.RedisConnectionString`

**Obecna wartość (szablon):**
```json
"RedisConnectionString": "YOUR_PRODUCTION_REDIS_CONNECTION_STRING"
```

**Co zmienić:**
- Wypełnij connection string do produkcyjnego Redis (np. Azure Redis Cache)
- Format: `your-redis.redis.cache.windows.net:6380,password=YOUR_PASSWORD,ssl=True,abortConnect=False`

**⚠️ KRYTYCZNE:**
- **NIE COMMITOWAĆ** hasła do repozytorium
- Użyj **Azure Key Vault** lub **Environment Variables**

---

### **7. AllowedHosts**

**Lokalizacja:** `AllowedHosts`

**Obecna wartość (szablon):**
```json
"AllowedHosts": "yourdomain.com,www.yourdomain.com"
```

**Co zmienić:**
- Zastąp `yourdomain.com` rzeczywistą domeną produkcyjną
- Dodaj wszystkie domeny, które będą hostować API

---

## 🔐 **BEZPIECZEŃSTWO - BEST PRACTICES**

### **1. Używaj Environment Variables dla Secrets**

**Zamiast hardcodować w appsettings.Production.json:**
```json
// ❌ ZŁE
"ConnectionStrings": {
  "DefaultConnection": "Server=...;Password=hardcoded_password"
}
```

**Użyj Environment Variables:**
```bash
# Azure App Service
ConnectionStrings__DefaultConnection="Server=...;Password=$DB_PASSWORD"

# Docker
-e ConnectionStrings__DefaultConnection="Server=...;Password=$DB_PASSWORD"
```

### **2. Użyj Azure Key Vault (ZALECANE)**

**Dla Azure deployments:**
```csharp
// Program.cs
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{builder.Configuration["KeyVault:VaultName"]}.vault.azure.net/"),
    new DefaultAzureCredential());
```

### **3. .gitignore**

**Upewnij się, że `appsettings.Production.json` jest w `.gitignore`** (lub użyj osobnego repozytorium dla secrets):

```gitignore
# .gitignore
appsettings.Production.json
appsettings.*.json
!appsettings.json
!appsettings.Development.json
```

**Alternatywnie:** Użyj `appsettings.Production.json.example` jako szablonu i nie commitować rzeczywistego pliku.

---

## ✅ **CHECKLIST PRZED WDROŻENIEM**

### **Konfiguracja:**

- [ ] ✅ Utworzono `appsettings.Production.json`
- [ ] ⚠️ Wypełniono `ConnectionStrings.DefaultConnection` (produkcyjna baza)
- [ ] ⚠️ Wygenerowano i wypełniono `Jwt.Key` (silny klucz, min. 32 znaki)
- [ ] ⚠️ Wypełniono `Jwt.Issuer` i `Jwt.Audience` (produkcyjne URL)
- [ ] ⚠️ Wypełniono `Stripe.SecretKey` (produkcyjny klucz `sk_live_...`)
- [ ] ⚠️ Wypełniono `Stripe.PublishableKey` (produkcyjny klucz `pk_live_...`)
- [ ] ⚠️ Wypełniono `Stripe.WebhookSecret` (produkcyjny webhook secret)
- [ ] ⚠️ Ustawiono `Stripe.Environment` na `"live"`
- [ ] ⚠️ Wypełniono `Cors.AllowedOrigins` (produkcyjne domeny HTTPS)
- [ ] ⚠️ Wypełniono `AllowedHosts` (produkcyjne domeny)
- [ ] ⚠️ Wypełniono `IdempotencySettings.RedisConnectionString` (produkcyjny Redis)
- [ ] ⚠️ Ustawiono `PaymentRetry.EnablePaymentProcessing` na `true` (dla produkcji)

### **Bezpieczeństwo:**

- [ ] ⚠️ **NIE COMMITOWAĆ** `appsettings.Production.json` do repozytorium (lub użyć Key Vault)
- [ ] ⚠️ Użyć **Environment Variables** lub **Azure Key Vault** dla wszystkich secrets
- [ ] ⚠️ Weryfikacja, że wszystkie hasła/klucze są **unikalne dla produkcji**
- [ ] ⚠️ Weryfikacja, że **JWT key** jest silny i losowy
- [ ] ⚠️ Weryfikacja, że **Stripe keys** są produkcyjne (`sk_live_`, `pk_live_`)

### **Weryfikacja:**

- [ ] ⚠️ Test na środowisku **staging** przed produkcją
- [ ] ⚠️ Weryfikacja **health checks** działają
- [ ] ⚠️ Weryfikacja **CORS** działa dla produkcyjnych domen
- [ ] ⚠️ Weryfikacja **Stripe webhooks** działają (test webhook z Stripe Dashboard)

---

## 📝 **PRZYKŁADOWA KONFIGURACJA Z ENVIRONMENT VARIABLES**

### **Azure App Service:**

```bash
# Connection String
ConnectionStrings__DefaultConnection="Server=sqlserver.database.windows.net;Database=Orbito_Prod;User Id=orbitouser;Password=$DB_PASSWORD;Encrypt=true"

# JWT
Jwt__Key="your_strong_random_key_here_minimum_32_characters"

# Stripe
Stripe__SecretKey="sk_live_..."
Stripe__PublishableKey="pk_live_..."
Stripe__WebhookSecret="whsec_..."

# CORS
Cors__AllowedOrigins__0="https://orbito.com"
Cors__AllowedOrigins__1="https://www.orbito.com"

# Redis
IdempotencySettings__RedisConnectionString="your-redis.redis.cache.windows.net:6380,password=$REDIS_PASSWORD,ssl=True"
```

### **Docker:**

```yaml
# docker-compose.yml
environment:
  - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
  - Jwt__Key=${JWT_SECRET_KEY}
  - Stripe__SecretKey=${STRIPE_SECRET_KEY}
  - Stripe__PublishableKey=${STRIPE_PUBLISHABLE_KEY}
  - Stripe__WebhookSecret=${STRIPE_WEBHOOK_SECRET}
```

---

## 🚀 **NASTĘPNE KROKI**

1. ✅ **Szablon utworzony** - `appsettings.Production.json`
2. ⚠️ **Wypełnij wszystkie wartości** oznaczone jako `YOUR_*` lub `CHANGE_THIS_*`
3. ⚠️ **Skonfiguruj Environment Variables** lub Azure Key Vault dla secrets
4. ⚠️ **Test na staging** przed produkcją
5. ⚠️ **Weryfikacja** wszystkich endpointów i integracji

---

**Ostatnia aktualizacja:** 2025-11-28  
**Status:** ⚠️ **WYMAGA WYPEŁNIENIA WARTOŚCI PRODUKCYJNYCH**

