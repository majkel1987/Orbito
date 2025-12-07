# 🔧 Implementacja Konfiguracji Produkcyjnej - Orbito

**Data:** 2025-11-28  
**Status:** ✅ Zaimplementowane

---

## 📋 **PODSUMOWANIE**

ASP.NET Core **automatycznie** ładuje pliki konfiguracyjne w następującej kolejności (pierwszy ma najwyższy priorytet):

1. **Environment Variables** (najwyższy priorytet)
2. **Command-line arguments**
3. **appsettings.{Environment}.json** (np. `appsettings.Production.json`)
4. **appsettings.json** (bazowy)

Gdzie `{Environment}` jest określony przez zmienną środowiskową `ASPNETCORE_ENVIRONMENT`.

---

## ✅ **CO ZOSTAŁO ZAIMPLEMENTOWANE**

### **1. CORS Configuration z appsettings.json**

**Zmiana w `Program.cs`:**
- ✅ CORS teraz używa konfiguracji z `appsettings.json` lub `appsettings.Production.json`
- ✅ Fallback do localhost dla developmentu (jeśli brak konfiguracji)

**Jak działa:**
```csharp
// Program.cs - linie 43-60
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:3000", "https://localhost:3000" }; // Fallback

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});
```

**Konfiguracja w appsettings.json:**
```json
"Cors": {
  "AllowedOrigins": [
    "http://localhost:3000",
    "https://localhost:3000"
  ]
}
```

**Konfiguracja w appsettings.Production.json:**
```json
"Cors": {
  "AllowedOrigins": [
    "https://yourdomain.com",
    "https://www.yourdomain.com"
  ]
}
```

---

## 🚀 **JAK USTAWIC ŚRODOWISKO PRODUKCYJNE**

### **Metoda 1: Environment Variable (ZALECANE)**

#### **Windows (PowerShell):**
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet run --project Orbito.API
```

#### **Windows (CMD):**
```cmd
set ASPNETCORE_ENVIRONMENT=Production
dotnet run --project Orbito.API
```

#### **Linux/Mac:**
```bash
export ASPNETCORE_ENVIRONMENT=Production
dotnet run --project Orbito.API
```

#### **Docker:**
```dockerfile
ENV ASPNETCORE_ENVIRONMENT=Production
```

#### **Azure App Service:**
W Azure Portal → Configuration → Application Settings:
```
ASPNETCORE_ENVIRONMENT = Production
```

---

### **Metoda 2: launchSettings.json (Development Only)**

**Plik:** `Orbito.API/Properties/launchSettings.json`

```json
{
  "profiles": {
    "Orbito.API": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Production"
      }
    }
  }
}
```

**⚠️ UWAGA:** To działa tylko lokalnie dla developmentu. W produkcji użyj Environment Variables.

---

## 🔐 **UŻYWANIE ENVIRONMENT VARIABLES DLA SECRETS**

### **Dlaczego Environment Variables?**

- ✅ **Bezpieczeństwo** - secrets nie są w plikach konfiguracyjnych
- ✅ **Elastyczność** - łatwa zmiana bez redeploy
- ✅ **Best Practice** - standard w chmurze (Azure, AWS, Docker)

### **Jak Ustawić Environment Variables**

#### **Windows (PowerShell):**
```powershell
# Connection String
$env:ConnectionStrings__DefaultConnection = "Server=...;Database=...;User Id=...;Password=..."

# JWT Key
$env:Jwt__Key = "your_strong_secret_key_here"

# Stripe Keys
$env:Stripe__SecretKey = "sk_live_..."
$env:Stripe__PublishableKey = "pk_live_..."
$env:Stripe__WebhookSecret = "whsec_..."

# CORS Origins (array)
$env:Cors__AllowedOrigins__0 = "https://yourdomain.com"
$env:Cors__AllowedOrigins__1 = "https://www.yourdomain.com"
```

#### **Linux/Mac:**
```bash
export ConnectionStrings__DefaultConnection="Server=...;Database=...;User Id=...;Password=..."
export Jwt__Key="your_strong_secret_key_here"
export Stripe__SecretKey="sk_live_..."
export Stripe__PublishableKey="pk_live_..."
export Stripe__WebhookSecret="whsec_..."
export Cors__AllowedOrigins__0="https://yourdomain.com"
export Cors__AllowedOrigins__1="https://www.yourdomain.com"
```

#### **Docker Compose:**
```yaml
services:
  orbito-api:
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
      - Jwt__Key=${JWT_SECRET_KEY}
      - Stripe__SecretKey=${STRIPE_SECRET_KEY}
      - Stripe__PublishableKey=${STRIPE_PUBLISHABLE_KEY}
      - Stripe__WebhookSecret=${STRIPE_WEBHOOK_SECRET}
      - Cors__AllowedOrigins__0=https://yourdomain.com
      - Cors__AllowedOrigins__1=https://www.yourdomain.com
```

#### **Azure App Service:**
W Azure Portal → Configuration → Application Settings:

```
ASPNETCORE_ENVIRONMENT = Production
ConnectionStrings__DefaultConnection = Server=...;Database=...;User Id=...;Password=...
Jwt__Key = your_strong_secret_key_here
Stripe__SecretKey = sk_live_...
Stripe__PublishableKey = pk_live_...
Stripe__WebhookSecret = whsec_...
Cors__AllowedOrigins__0 = https://yourdomain.com
Cors__AllowedOrigins__1 = https://www.yourdomain.com
```

---

## 🔑 **AZURE KEY VAULT INTEGRATION (OPCJONALNIE)**

### **Dlaczego Azure Key Vault?**

- ✅ **Centralne zarządzanie secrets**
- ✅ **Automatyczna rotacja kluczy**
- ✅ **Audit log** - kto i kiedy użył secret
- ✅ **Integracja z Azure App Service**

### **Implementacja Azure Key Vault**

#### **Krok 1: Zainstaluj NuGet Package**

```bash
dotnet add Orbito.API/Orbito.API.csproj package Azure.Extensions.AspNetCore.Configuration.Secrets
dotnet add Orbito.API/Orbito.API.csproj package Azure.Identity
```

#### **Krok 2: Dodaj do Program.cs**

```csharp
// Program.cs - na początku, przed builder.Configuration
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add Azure Key Vault if in Production
if (builder.Environment.IsProduction())
{
    var keyVaultName = builder.Configuration["KeyVault:VaultName"];
    if (!string.IsNullOrEmpty(keyVaultName))
    {
        var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
        builder.Configuration.AddAzureKeyVault(
            keyVaultUri,
            new DefaultAzureCredential());
    }
}
```

#### **Krok 3: Konfiguracja w appsettings.Production.json**

```json
{
  "KeyVault": {
    "VaultName": "orbito-keyvault"
  }
}
```

#### **Krok 4: Azure App Service Configuration**

W Azure Portal → Configuration → Identity:
- ✅ **Enable System Assigned Managed Identity**

W Azure Key Vault → Access Policies:
- ✅ **Add Access Policy** dla Managed Identity z App Service
- ✅ **Permissions:** Get, List dla Secrets

---

## 📝 **STRUKTURA KONFIGURACJI**

### **Kolejność Ładowania (od najwyższego do najniższego priorytetu):**

1. **Environment Variables** (nadpisuje wszystko)
2. **Command-line arguments** (`--key=value`)
3. **appsettings.{Environment}.json** (np. `appsettings.Production.json`)
4. **appsettings.json** (bazowy)

### **Przykład:**

**appsettings.json:**
```json
{
  "Jwt": {
    "Key": "development_key"
  }
}
```

**appsettings.Production.json:**
```json
{
  "Jwt": {
    "Key": "production_key_from_file"
  }
}
```

**Environment Variable:**
```bash
export Jwt__Key="production_key_from_env"
```

**Wynik:** Aplikacja użyje `"production_key_from_env"` (Environment Variable ma najwyższy priorytet)

---

## ✅ **WERYFIKACJA KONFIGURACJI**

### **Test 1: Sprawdź Środowisko**

Dodaj endpoint do weryfikacji konfiguracji:

```csharp
// Program.cs - przed app.Run()
app.MapGet("/config/environment", () => Results.Ok(new
{
    environment = app.Environment.EnvironmentName,
    isProduction = app.Environment.IsProduction(),
    isDevelopment = app.Environment.IsDevelopment()
}));

app.MapGet("/config/cors", (IConfiguration config) => Results.Ok(new
{
    allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>()
}));
```

### **Test 2: Sprawdź Logi**

ASP.NET Core loguje informacje o załadowanych źródłach konfiguracji:

```
info: Microsoft.Extensions.Configuration.ConfigurationProvider[0]
      Loaded configuration from appsettings.json
info: Microsoft.Extensions.Configuration.ConfigurationProvider[0]
      Loaded configuration from appsettings.Production.json
```

---

## 🚀 **DEPLOYMENT SCENARIUSZE**

### **Scenariusz 1: Azure App Service**

1. **Utwórz App Service** w Azure Portal
2. **Ustaw Environment Variable:**
   ```
   ASPNETCORE_ENVIRONMENT = Production
   ```
3. **Dodaj Application Settings** (secrets):
   ```
   ConnectionStrings__DefaultConnection = ...
   Jwt__Key = ...
   Stripe__SecretKey = ...
   ```
4. **Deploy aplikacji** (GitHub Actions, Azure DevOps, VS Code)

**✅ appsettings.Production.json zostanie automatycznie załadowany**

---

### **Scenariusz 2: Docker**

**Dockerfile:**
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Orbito.API/Orbito.API.csproj", "Orbito.API/"]
# ... rest of build

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "Orbito.API.dll"]
```

**docker-compose.yml:**
```yaml
services:
  orbito-api:
    build: .
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
      - Jwt__Key=${JWT_SECRET_KEY}
      - Stripe__SecretKey=${STRIPE_SECRET_KEY}
    ports:
      - "8080:80"
```

---

### **Scenariusz 3: Lokalne Testowanie Produkcji**

**PowerShell:**
```powershell
# Ustaw środowisko
$env:ASPNETCORE_ENVIRONMENT = "Production"

# Ustaw secrets (opcjonalnie - można też użyć appsettings.Production.json)
$env:ConnectionStrings__DefaultConnection = "Server=...;Database=...;..."
$env:Jwt__Key = "test_production_key"

# Uruchom aplikację
dotnet run --project Orbito.API
```

---

## 📋 **CHECKLIST IMPLEMENTACJI**

### **✅ Zaimplementowane:**

- [x] ✅ CORS używa konfiguracji z appsettings.json
- [x] ✅ appsettings.Production.json utworzony
- [x] ✅ .gitignore zaktualizowany (appsettings.json nie jest commitowany)

### **⚠️ Do Wykonania:**

- [ ] ⚠️ Wypełnić appsettings.Production.json wartościami produkcyjnymi
- [ ] ⚠️ Ustawić `ASPNETCORE_ENVIRONMENT=Production` w środowisku produkcyjnym
- [ ] ⚠️ Skonfigurować Environment Variables dla secrets (lub Azure Key Vault)
- [ ] ⚠️ Przetestować na środowisku staging przed produkcją
- [ ] ⚠️ Weryfikacja, że wszystkie konfiguracje działają poprawnie

---

## 🔍 **DEBUGGING KONFIGURACJI**

### **Sprawdź Załadowane Konfiguracje:**

Dodaj endpoint do debugowania:

```csharp
// Program.cs - tylko dla Development
if (app.Environment.IsDevelopment())
{
    app.MapGet("/config/debug", (IConfiguration config) => Results.Ok(new
    {
        environment = app.Environment.EnvironmentName,
        connectionString = config.GetConnectionString("DefaultConnection")?.Substring(0, 20) + "...",
        jwtKey = config["Jwt:Key"]?.Substring(0, 10) + "...",
        stripeEnvironment = config["Stripe:Environment"],
        corsOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>(),
        allKeys = config.AsEnumerable().Select(kvp => new { key = kvp.Key, value = kvp.Value?.Substring(0, Math.Min(20, kvp.Value?.Length ?? 0)) + "..." })
    }));
}
```

---

## 📚 **DODATKOWE ZASOBY**

### **Oficjalna Dokumentacja:**

- [ASP.NET Core Configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)
- [Environment Variables](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/#environment-variables)
- [Azure Key Vault](https://learn.microsoft.com/en-us/aspnet/core/security/key-vault-configuration)

### **Best Practices:**

1. ✅ **Używaj Environment Variables** dla secrets w produkcji
2. ✅ **Nie commituj** appsettings.Production.json z prawdziwymi wartościami
3. ✅ **Użyj Azure Key Vault** dla centralnego zarządzania secrets
4. ✅ **Testuj konfigurację** na staging przed produkcją
5. ✅ **Weryfikuj** wszystkie konfiguracje po deploymencie

---

**Ostatnia aktualizacja:** 2025-11-28  
**Status:** ✅ **Zaimplementowane - Gotowe do użycia**

