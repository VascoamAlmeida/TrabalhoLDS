# MachineHub

Academic group project developed for the **Software Development Laboratory** course.

MachineHub is a machinery sales/rental platform composed of a .NET web/API application and a Flutter mobile client.

## Repository structure

```text
machinehub/
├── backend-web/   # ASP.NET Core Web/API, Razor/MVC, EF Core, tests and Playwright
└── mobile/        # Flutter mobile application and tests
```

## Main technologies

- C# / ASP.NET Core 8
- Entity Framework Core
- SQL Server / SQLite
- JWT authentication
- Swagger / OpenAPI
- Razor Pages / MVC
- Docker
- Flutter / Dart
- OneSignal integration
- xUnit / integration tests
- Playwright system tests

## Public repository cleanup

This repository was prepared from the final academic milestone. Delivery-only material, personal/group administration documents, generated reports, runtime uploads and credentials were removed.

Service-specific secrets are intentionally not stored in source control. See the configuration files in each component before running the project.

## Configuration

- Backend configuration: [`backend-web/CONFIGURATION.md`](backend-web/CONFIGURATION.md)
- Mobile configuration: [`mobile/CONFIGURATION.md`](mobile/CONFIGURATION.md)

No real JWT signing keys, database passwords, Firebase configuration or privileged OneSignal REST keys are stored in the public source.

## Testing

See [`TESTING.md`](TESTING.md) for the unit, integration, Playwright and Flutter test commands.

## Project status

The code is preserved close to the final academic delivery. Public-repository cleanup, security/configuration review, backend/API review, mobile review and static test validation have been completed.

A full .NET/Flutter SDK build and test run should still be performed locally before tagging the final portfolio release. The remaining repository work is presentation/documentation polish rather than new application functionality.
