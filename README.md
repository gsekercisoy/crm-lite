# CRM Lite — Full-Stack Hiring Showcase

[🇹🇷 Türkçe için README_tr.md](./README_tr.md)

## Stack
- **API:** .NET 8 Web API, EF Core 8 (SQL Server + InMemory), JWT Bearer (SHA256+salt, role claim `Admin`/`Sales`), Swagger (dev), open CORS
- **Web:** Vite + React 18 + react-router + axios (`/api` proxy)
- **Tests:** xUnit + EF InMemory + `Microsoft.AspNetCore.Mvc.Testing` (8 tests)

## Run locally
```bash
# API (InMemory, no SQL Server needed)
cd crm-api
USE_INMEMORY=true dotnet run   # http://localhost:5000 (dev swagger at /swagger)

# Web
cd crm-web
npm install
npm run dev    # http://localhost:5173, /api proxied to :5000
```

## API summary
- `POST /api/auth/register`, `POST /api/auth/login`
- `GET/POST /api/customers`, `GET /api/customers/{id}` (with deals)
- `GET /api/deals?stage=`, `POST /api/deals`, `POST /api/deals/{id}/stage`
  - Closed deals (`Won`/`Lost`) are immutable → `400`
- `GET/POST /api/deals/{id}/activities`
- `GET /api/pipeline` (per-stage count + total, plus grand totals)

## Docker
```bash
docker compose up --build
# web :5173, api :5000, sqlserver :1433
```

## Verify
```bash
dotnet test crm-api.Tests/crm-api.Tests.csproj
cd crm-web && npm run build
```

No secrets are committed (dev-only placeholder keys).
