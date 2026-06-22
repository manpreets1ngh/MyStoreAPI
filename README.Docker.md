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

## Database schema / migrations
EF Core migrations exist for both DbContexts (under `Migrations/MyStore` and
`Migrations/Identity`) and are **applied automatically on container startup** — the
`api` service sets `RunMigrationsAtStartup=true`, which makes `Program.cs` run
`Database.Migrate()` for both contexts before serving traffic. So on `docker compose up`
the schema (all `Products`/`Orders`/`AspNet*`/`Addresses`/… tables) is created for you.

> Auto-migration is **opt-in** via `RunMigrationsAtStartup`. It stays off by default so
> the integration tests and a plain `dotnet run` don't try to reach a database on boot.

The two contexts share one database but keep separate migration histories
(`__EFMigrationsHistory` for the app, `__EFMigrationsHistory_Identity` for Identity).

To add a new migration after changing the model:

```bash
dotnet ef migrations add <Name> --context MyStoreAPIContext        --output-dir Migrations/MyStore
dotnet ef migrations add <Name> --context MyStoreAPIIdentityContext --output-dir Migrations/Identity
```

## Quick smoke test
With the stack running:

```bash
# Swagger should return HTTP 200
curl -i http://localhost:8080/swagger/index.html

# register a user — writes to the database (expect 200 "User created successfully!")
curl -i -X POST http://localhost:8080/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"manny","email":"manny@example.com","password":"P@ssw0rd!","firstName":"Manny","lastName":"S"}'

# log in — reads the user back and returns a JWT (expect 200 with a token)
curl -i -X POST http://localhost:8080/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"manny@example.com","password":"P@ssw0rd!"}'

# register-admin should be locked down (401 without auth) — the fix from PR #5
curl -i -X POST http://localhost:8080/api/auth/register-admin \
  -H "Content-Type: application/json" \
  -d '{"username":"a","email":"a@b.com","password":"P@ssw0rd!","firstName":"A","lastName":"B"}'
```

> Inspect the tables directly with: `docker compose exec db psql -U postgres -d mystore -c "\dt"`
