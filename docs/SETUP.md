# Local Setup


1. Copy the configuration template:

```powershell
Copy-Item appsettings.json.example appsettings.json
```

2. Use `dotnet user-secrets` to store local credentials:

```powershell
dotnet user-secrets init -p MetroSol.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=MetroSolAPI;..."
dotnet user-secrets set "Jwt:Secret" "your-secret-here"
```

3. Alternative: create `appsettings.local.json` (not versioned) for local values.
