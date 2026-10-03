# Mobile configuration

The public repository does not contain service credentials from the original academic delivery.

## API

The mobile client reads the API base URL from a Dart compile-time variable:

```bash
flutter run --dart-define=API_BASE_URL=http://10.0.2.2:5079/api
```

`10.0.2.2` is the Android Emulator address for the host machine and remains the development default.

The Android debug manifest allows clear-text HTTP only for local development. A release deployment should use an HTTPS API URL.

## OneSignal

The OneSignal application ID is optional and is supplied at build/run time:

```bash
flutter run \
  --dart-define=ONESIGNAL_APP_ID=your-app-id
```

The OneSignal REST API key is deliberately **not** used by the mobile client. Privileged notification sending must be performed by a trusted backend service rather than embedding the REST key in the application binary.

## Firebase

The original academic delivery included service-specific Firebase configuration. It is not published.

If Firebase configuration is required for a local environment, copy:

`android/app/google-services.example.json` -> `android/app/google-services.json`

and replace the placeholders with values from your own Firebase project. `google-services.json` is ignored by Git.

## End-to-end tests

Real test accounts are not stored in source control. Supply local E2E credentials when running the integration tests:

```bash
flutter test integration_test/app_test.dart \
  --dart-define=E2E_TEST_EMAIL=test-account@example.com \
  --dart-define=E2E_TEST_PASSWORD=your-local-test-password
```

## Client profile

The original workflow creates a user account first and the detailed `Client` profile separately. The mobile app therefore requires a client profile to exist before an order can be created. In this preserved academic version, profile completion remains available through the Web application.
