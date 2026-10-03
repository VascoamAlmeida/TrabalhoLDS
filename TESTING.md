# Testing

The final academic delivery contains four test layers: .NET unit tests, .NET API integration tests, Playwright web tests, and Flutter tests.

## Backend / Web

The solution targets the .NET SDK specified in `global.json`.

```bash
dotnet restore LojaMaquinas.sln
dotnet build LojaMaquinas.sln -c Release --no-restore
dotnet test Tests/Tests.csproj -c Release
dotnet test IntegrationTests/IntegrationTests.csproj -c Release
```

The integration tests use SQLite in-memory and a test-only JWT signing key. They do not require the development SQL Server connection string.

## Playwright

From `backend-web/Playwright`:

```bash
npm ci
npx playwright install
npm test
```

The web application must be running at the `baseURL` configured in `playwright.config.js`. Tests that need administrative actions require a suitable local/test administrator account supplied through environment variables; never commit those credentials.

```bash
export E2E_BASE_URL=http://localhost:5079
export E2E_ADMIN_EMAIL=your-admin@example.test
export E2E_ADMIN_PASSWORD=your-admin-test-password
npm test
```

## Flutter

From `mobile`:

```bash
flutter pub get
flutter analyze
flutter test
```

The E2E integration test requires a running backend and local credentials supplied at runtime:

```bash
flutter test integration_test \
  --dart-define=E2E_TEST_EMAIL=your-test-user@example.test \
  --dart-define=E2E_TEST_PASSWORD=your-test-password
```

For Android Emulator development, the default API address is `http://10.0.2.2:5079/api`. Override it with `--dart-define=API_BASE_URL=...` when necessary.

## Test inventory

The cleaned source currently contains:

- 177 xUnit `[Fact]` tests across the unit and API integration projects;
- 18 Playwright spec files;
- 20 Flutter `test` / `testWidgets` declarations across unit/widget and integration tests.

These numbers describe the tests present in source control, not a claim that they were executed in the cleanup environment.

## Note on this cleaned snapshot

The public snapshot was statically validated after cleanup, including JSON/XML/YAML parsing, solution/project references, JavaScript syntax, Dart local imports and basic source-structure checks. A full SDK build/test run must still be performed on a machine with .NET 8 and Flutter installed before tagging a release.
