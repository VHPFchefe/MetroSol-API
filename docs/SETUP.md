# Setup Local


1. Copie o template de configuração:

```powershell
Copy-Item appsettings.json.example appsettings.json
```

2. Use `dotnet user-secrets` para armazenar credenciais locais:

```powershell
dotnet user-secrets init -p MetroSol.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=MetroSolAPI;..."
dotnet user-secrets set "Jwt:Secret" "seu-secret-aqui"
```

3. Alternativa: crie `appsettings.local.json` (não versionado) para valores locais.
