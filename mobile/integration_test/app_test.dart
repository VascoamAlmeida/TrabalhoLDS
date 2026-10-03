import 'package:flutter/material.dart';
import 'package:flutter_test/flutter_test.dart';
import 'package:integration_test/integration_test.dart';
import 'package:loja_maquinas_mobile/main.dart' as app;

const String _testEmail = String.fromEnvironment('E2E_TEST_EMAIL');
const String _testPassword = String.fromEnvironment('E2E_TEST_PASSWORD');

void main() {
  IntegrationTestWidgetsFlutterBinding.ensureInitialized();

  if (_testEmail.isEmpty || _testPassword.isEmpty) {
    testWidgets(
      'E2E tests require local credentials',
      (tester) async {},
      skip: 'Set E2E_TEST_EMAIL and E2E_TEST_PASSWORD with --dart-define.',
    );
    return;
  }

  Future<void> doLogin(WidgetTester tester) async {
    final emailField = find.byType(TextField).first;
    await tester.enterText(emailField, _testEmail);

    final passwordField = find.byType(TextField).last;
    await tester.enterText(passwordField, _testPassword);

    await tester.tap(find.widgetWithText(ElevatedButton, 'Entrar'));
    await tester.pumpAndSettle(const Duration(seconds: 5));
  }

  group('Autenticação', () {
    testWidgets('login com sucesso', (tester) async {
      app.main();
      await tester.pumpAndSettle();
      await doLogin(tester);
      expect(find.text('Bem-vindo!'), findsOneWidget);
    });

    testWidgets('login inválido mantém o utilizador no ecrã de login',
        (tester) async {
      app.main();
      await tester.pumpAndSettle();

      await tester.enterText(find.byType(TextField).first, 'invalid@example.test');
      await tester.enterText(find.byType(TextField).last, 'invalid-password');
      await tester.tap(find.widgetWithText(ElevatedButton, 'Entrar'));
      await tester.pumpAndSettle(const Duration(seconds: 3));

      expect(find.widgetWithText(ElevatedButton, 'Entrar'), findsOneWidget);
      expect(find.text('Bem-vindo!'), findsNothing);
    });

    testWidgets('logout regressa ao login', (tester) async {
      app.main();
      await tester.pumpAndSettle();
      await doLogin(tester);

      await tester.tap(find.byIcon(Icons.logout));
      await tester.pumpAndSettle();

      expect(find.widgetWithText(ElevatedButton, 'Entrar'), findsOneWidget);
      expect(find.text('Bem-vindo!'), findsNothing);
    });
  });

  group('Catálogo', () {
    testWidgets('abre catálogo e detalhes de produto', (tester) async {
      app.main();
      await tester.pumpAndSettle();
      await doLogin(tester);

      await tester.tap(find.text('Produtos'));
      await tester.pumpAndSettle(const Duration(seconds: 3));
      expect(find.text('Catálogo'), findsWidgets);

      final detailsButton = find.text('Detalhes');
      if (detailsButton.evaluate().isNotEmpty) {
        await tester.tap(detailsButton.first);
        await tester.pumpAndSettle();
        expect(find.text('Detalhes do Produto'), findsOneWidget);
      }
    });
  });
}
