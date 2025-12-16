# Migracje Bazy Danych

## Utworzenie nowej migracji

Aby utworzyć nową migrację, uruchom następującą komendę w terminalu z katalogu głównego projektu:

```bash
dotnet ef migrations add InitialCreate --project Orbito.Infrastructure --startup-project Orbito.API
```

## Zastosowanie migracji do bazy danych

```bash
dotnet ef database update --project Orbito.Infrastructure --startup-project Orbito.API
```

## Usunięcie ostatniej migracji

```bash
dotnet ef migrations remove --project Orbito.Infrastructure --startup-project Orbito.API
```

## Wymagania

- .NET 9 SDK
- Entity Framework Core Tools: `dotnet tool install --global dotnet-ef`
- SQL Server (lokalny lub zdalny)
- Poprawnie skonfigurowany connection string w `appsettings.json`





