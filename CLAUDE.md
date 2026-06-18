# MyStoreAPI

## What this is
An ASP.NET Core 8 (`net8.0`) Web API. It uses:
- **EF Core** with the **Npgsql** (PostgreSQL) provider for data access
  (`MyStoreAPIContext`) and ASP.NET Core Identity (`MyStoreAPIIdentityContext`).
- **JWT bearer authentication**, with role-based authorization policies
  (`UserPolicy` → `User` role, `AdminPolicy` → `Admin` role) configured in
  [Program.cs](Program.cs).

## Build & test
- Build: `dotnet build`
- Test: `dotnet test`

## Layout
- `Controllers/` — API controllers (e.g. `AuthController` for auth/registration).
- `Services/` — business-logic services and their interfaces under `Services/Interface/`.
- `Models/` — entities, request, and response models.
- `Areas/Identity/` — Identity data context and user model.
- `Program.cs` — host setup: DbContexts, Identity, JWT auth, authorization policies, CORS.
- `MyStoreAPI.Tests/` — xUnit integration tests that boot the app in memory via
  `WebApplicationFactory`.

## Notes
- JWT settings come from the `JWT` config section (or `JWT_*` environment
  variables). `JWT:Secret` must be a valid Base64 string — it is decoded with
  `Convert.FromBase64String` when configuring the signing key.
