# Running MyStoreAPI locally with Docker

This is the **Phase 1** local dev setup: the API and a PostgreSQL database, both
running as containers via Docker Compose. It's the foundation for the later
Kubernetes phase.

## Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running.

## Start everything
From the repo root:

```bash
docker compose up --build
```

This will:
1. Build the API image from the `Dockerfile`.
2. Start a `postgres:16` container (`db`) and wait until it's healthy.
3. Start the API container (`api`) wired to that database.

When it's up:
- **API / Swagger UI:** http://localhost:8080/swagger
- **Postgres (from the host):** `localhost:5432` — user `postgres`, password `postgres`, db `mystore`

## Stop
```bash
docker compose down        # stop and remove containers
docker compose down -v     # also delete the database volume (fresh start)
```

## How configuration is injected
The API reads its settings from environment variables set in `docker-compose.yml`
(no real secrets in the repo):

| Variable | Purpose |
|---|---|
| `ConnectionStrings__MyStoreAPIContext` | Main EF Core DbContext → Postgres |
| `ConnectionStrings__MyStoreAPIIdentityContextConnection` | Identity DbContext → Postgres |
| `JWT_Secret` | Base64 signing key (dev-only throwaway value) |
| `JWT_ValidIssuer` / `JWT_ValidAudience` | Token issuer/audience |
| `ASPNETCORE_ENVIRONMENT=Development` | Enables Swagger UI |

> The `__` (double underscore) in the connection-string names is how .NET maps an
> environment variable onto a nested config key (`ConnectionStrings:MyStoreAPIContext`).

## ⚠️ Known limitation: database schema / migrations
The app will **start** and Swagger will load, but **endpoints that hit the database
will fail** until the schema exists. This repo currently has **no EF Core migrations**
(the `Migrations/` folder is empty), so the tables aren't created automatically.

To exercise DB-backed endpoints you'll need to add and apply migrations, e.g.:

```bash
# add a migration for each DbContext (run on the host, with the dotnet-ef tool)
dotnet ef migrations add Initial --context MyStoreAPIContext
dotnet ef migrations add InitialIdentity --context MyStoreAPIIdentityContext
```

…and then apply them (either via `dotnet ef database update` against the exposed
`localhost:5432`, or by wiring `context.Database.Migrate()` into startup). That's a
good follow-up task once the container plumbing is confirmed working.

## Quick smoke test
With the stack running:

```bash
# Swagger should return HTTP 200
curl -i http://localhost:8080/swagger/index.html

# register-admin should be locked down (401 without auth) — the fix from PR #5
curl -i -X POST http://localhost:8080/api/auth/register-admin \
  -H "Content-Type: application/json" \
  -d '{"username":"a","email":"a@b.com","password":"P@ssw0rd!","firstName":"A","lastName":"B"}'
```
