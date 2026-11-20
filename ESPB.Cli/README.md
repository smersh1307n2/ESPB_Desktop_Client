# ESPB CLI Translator

Cross-platform .NET 10 console client for the existing ESPB Translation API. It uses the same `/translate` request contract and optimization defaults as the WPF client.

## Build

```bash
dotnet build ESPB.Cli/ESPB.Cli.csproj -c Release
```

## Configure authentication

Set the API key as an environment variable so it is not stored in shell history:

PowerShell:
```powershell
$env:ESPB_API_KEY = "your-api-key"
```

Bash:
```bash
export ESPB_API_KEY="your-api-key"
```

## Translate

```bash
dotnet run --project ESPB.Cli -- --bc path/to/module.bc --out path/to/test.espb
```

Optional files use the same fields as the desktop client:

```bash
dotnet run --project ESPB.Cli -- --bc module.bc --hints module.hints --idf-symbols idf.sym --custom-symbols custom.sym
```

Check quota:

```bash
dotnet run --project ESPB.Cli -- --limits
```

The API key is sent only as a Bearer authorization header. The client does not persist credentials.
