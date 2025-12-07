# 🚀 Quick Start - Konfiguracja Produkcyjna

**Data:** 2025-11-28  
**Status:** ✅ Gotowe do użycia

---

## ✅ **CO ZOSTAŁO ZAIMPLEMENTOWANE**

### **1. CORS z Konfiguracji**
- ✅ `Program.cs` teraz używa `appsettings.json` / `appsettings.Production.json` dla CORS
- ✅ Automatyczny fallback do localhost dla developmentu

### **2. Pliki Konfiguracyjne**
- ✅ `appsettings.Production.json` - szablon utworzony
- ✅ `.gitignore` - appsettings.json nie jest commitowany

---

## 🎯 **JAK TO DZIAŁA**

### **Automatyczne Ładowanie Konfiguracji**

ASP.NET Core **automatycznie** ładuje pliki w tej kolejności:

1. **Environment Variables** (najwyższy priorytet) ⭐
2. **Command-line arguments**
3. **appsettings.{Environment}.json** (np. `appsettings.Production.json`)
4. **appsettings.json** (bazowy)

Gdzie `{Environment}` = wartość zmiennej `ASPNETCORE_ENVIRONMENT`.

---

## 🚀 **JAK URUCHOMIĆ W PRODUKCJI**

### **Krok 1: Ustaw Środowisko**

**Windows (PowerShell):**
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
```

**Windows (CMD):**
```cmd
set ASPNETCORE_ENVIRONMENT=Production
```

**Linux/Mac:**
```bash
export ASPNETCORE_ENVIRONMENT=Production
```

### **Krok 2: Wypełnij appsettings.Production.json**

Otwórz `Orbito.API/appsettings.Production.json` i zastąp:
- `YOUR_PRODUCTION_SERVER` → adres serwera SQL
- `YOUR_USER` → użytkownik bazy danych
- `YOUR_PASSWORD` → hasło (⚠️ użyj Environment Variable!)
- `CHANGE_THIS_TO_STRONG_SECRET_KEY...` → silny JWT key (min. 32 znaki)
- `sk_live_YOUR_PRODUCTION_SECRET_KEY` → produkcyjny Stripe key
- `yourdomain.com` → Twoja domena

### **Krok 3: Uruchom Aplikację**

```bash
dotnet run --project Orbito.API
```

**✅ Aplikacja automatycznie załaduje `appsettings.Production.json`!**

---

## 🔐 **BEZPIECZEŃSTWO - Użyj Environment Variables**

### **Zamiast hardcodować w appsettings.Production.json:**

```powershell
# Connection String
$env:ConnectionStrings__DefaultConnection = "Server=...;Database=...;User Id=...;Password=..."

# JWT Key
$env:Jwt__Key = "your_strong_secret_key_here_minimum_32_characters"

# Stripe Keys
$env:Stripe__SecretKey = "sk_live_..."
$env:Stripe__PublishableKey = "pk_live_..."
$env:Stripe__WebhookSecret = "whsec_..."

# CORS Origins
$env:Cors__AllowedOrigins__0 = "https://yourdomain.com"
$env:Cors__AllowedOrigins__1 = "https://www.yourdomain.com"
```

**✅ Environment Variables mają wyższy priorytet niż appsettings.Production.json**

---

## 📋 **PRZYKŁADOWA KONFIGURACJA**

### **appsettings.Production.json (szablon):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=Orbito_Prod;..."
  },
  "Jwt": {
    "Key": "CHANGE_THIS_TO_STRONG_SECRET_KEY..."
  },
  "Stripe": {
    "SecretKey": "sk_live_YOUR_KEY",
    "Environment": "live"
  },
  "Cors": {
    "AllowedOrigins": [
      "https://yourdomain.com"
    ]
  }
}
```

### **Environment Variables (nadpisują appsettings.Production.json):**
```powershell
$env:ConnectionStrings__DefaultConnection = "Server=prod-server;Database=Orbito_Prod;..."
$env:Jwt__Key = "actual_production_secret_key"
$env:Stripe__SecretKey = "sk_live_actual_key"
```

**Wynik:** Aplikacja użyje wartości z Environment Variables (bezpieczniejsze!)

---

## ✅ **WERYFIKACJA**

### **Sprawdź Środowisko:**

Dodaj do `Program.cs` (tylko dla debugowania):

```csharp
// Tylko dla Development
if (app.Environment.IsDevelopment())
{
    app.MapGet("/config/test", () => Results.Ok(new
    {
        environment = app.Environment.EnvironmentName,
        isProduction = app.Environment.IsProduction(),
        corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    }));
}
```

**Uruchom:** `http://localhost:5211/config/test`

---

## 🎯 **PODSUMOWANIE**

✅ **Zaimplementowane:**
- CORS używa konfiguracji z appsettings
- appsettings.Production.json utworzony
- .gitignore zaktualizowany

⚠️ **Do wykonania:**
1. Ustaw `ASPNETCORE_ENVIRONMENT=Production`
2. Wypełnij appsettings.Production.json (lub użyj Environment Variables)
3. Uruchom aplikację

**✅ Gotowe! Aplikacja automatycznie użyje konfiguracji produkcyjnej!**

---

**Szczegóły:** Zobacz `PRODUCTION_CONFIGURATION_IMPLEMENTATION.md`

