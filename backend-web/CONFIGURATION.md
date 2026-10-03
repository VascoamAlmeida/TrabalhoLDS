# Backend configuration

The public repository does not contain database passwords or JWT signing keys.

## SQL Server development

Start SQL Server:

```bash
export MSSQL_SA_PASSWORD='replace-with-a-strong-local-password'
docker compose -f src/LojaMaquinas.WebApp/docker-compose.yml up -d
```

Configure the ASP.NET Core application with environment variables (or .NET user-secrets):

```bash
export JwtSettings__Key='replace-with-a-random-key-of-at-least-32-bytes'
export ConnectionStrings__LojaMaquinas='Server=localhost,1433;Database=LojaMaquinas;User Id=sa;Password=replace-with-a-strong-local-password;TrustServerCertificate=True'
```

Optional CORS origins can be configured using indexed environment variables:

```bash
export Cors__AllowedOrigins__0='http://localhost:5079'
```

The sample values in `.env.example` are placeholders only. ASP.NET Core does not automatically load that file.

## SQLite

The production configuration included with the academic project uses SQLite. Override `UseSQLite` and the corresponding connection string as needed for your environment.

## JWT

`JwtSettings__Key` is mandatory and must be at least 32 bytes. The application intentionally fails at startup when the key is missing rather than silently using a committed default.
