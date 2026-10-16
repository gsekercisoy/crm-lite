# CRM Lite — Full-Stack İşe Alım Vitrini

[🇬🇧 English README](./README.md)

## Teknolojiler
- **API:** .NET 8 Web API, EF Core 8 (SQL Server + InMemory), JWT Bearer (SHA256+salt, rol claim `Admin`/`Sales`), Swagger (dev), açık CORS
- **Web:** Vite + React 18 + react-router + axios (`/api` proxy)
- **Test:** xUnit + EF InMemory + `Microsoft.AspNetCore.Mvc.Testing` (8 test)

## Yerelde çalıştırma
```bash
# API (InMemory, SQL Server gerekmez)
cd crm-api
USE_INMEMORY=true dotnet run   # http://localhost:5000 (swagger: /swagger)

# Web
cd crm-web
npm install
npm run dev    # http://localhost:5173, /api -> :5000'a proxy'lenir
```

## API özeti
- `POST /api/auth/register`, `POST /api/auth/login`
- `GET/POST /api/customers`, `GET /api/customers/{id}` (fırsatlarıyla)
- `GET /api/deals?stage=`, `POST /api/deals`, `POST /api/deals/{id}/stage`
  - Kapanmış deal (`Won`/`Lost`) değişmez → `400`
- `GET/POST /api/deals/{id}/activities`
- `GET /api/pipeline` (stage bazında adet + toplam tutar)

## Docker
```bash
docker compose up --build
# web :5173, api :5000, sqlserver :1433
```

## Doğrulama
```bash
dotnet test crm-api.Tests/crm-api.Tests.csproj
cd crm-web && npm run build
```

Repoda secret yok (yalnızca geliştirme amaçlı örnek anahtarlar).
