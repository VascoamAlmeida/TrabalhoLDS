import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:onesignal_flutter/onesignal_flutter.dart';
import 'package:provider/provider.dart';

import 'core/theme/app_theme.dart';
import 'models/product.dart';
import 'models/stock.dart';
import 'providers/auth_provider.dart';
import 'providers/client_provider.dart';
import 'providers/orders_provider.dart';
import 'providers/products_provider.dart';
import 'screens/auth/login_screen.dart';
import 'screens/auth/register_screen.dart';
import 'screens/order/checkout_screen.dart';
import 'screens/order/create_order_screen.dart';
import 'screens/orderList.dart';
import 'screens/product/catalog_screen.dart';
import 'screens/product/details_screen.dart';
import 'widgets/main_layout.dart';

void main() async {
  WidgetsFlutterBinding.ensureInitialized();

  if (!kIsWeb) {
    const oneSignalAppId = String.fromEnvironment('ONESIGNAL_APP_ID');
    if (oneSignalAppId.isNotEmpty) {
      OneSignal.initialize(oneSignalAppId);
      OneSignal.Notifications.requestPermission(true);
    }
  }

  runApp(const MyApp());
}

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MultiProvider(
      providers: [
        ChangeNotifierProvider(create: (_) => AuthProvider()),
        ChangeNotifierProvider(create: (_) => ProductsProvider()),
        ChangeNotifierProvider(create: (_) => OrdersProvider()),
        ChangeNotifierProvider(create: (_) => ClientProvider()),
      ],
      child: MaterialApp(
        title: 'MachineHub',
        theme: AppTheme.lightTheme,
        home: const _AuthGate(),
        debugShowCheckedModeBanner: false,
        routes: {
          '/login': (context) => const LoginScreen(),
          '/register': (context) => const RegisterScreen(),
          '/home': (context) => const MainLayout(),
          '/catalog': (context) => const CatalogScreen(),
          '/product-details': (context) {
            final product = ModalRoute.of(context)!.settings.arguments as Product;
            return ProductDetailsScreen(product: product);
          },
          '/create-order': (context) {
            final args = ModalRoute.of(context)!.settings.arguments
                as Map<String, dynamic>;
            return CreateOrderScreen(
              stock: args['stock'] as Stock,
              product: args['product'] as Product,
            );
          },
          '/checkout': (context) {
            final orderId = ModalRoute.of(context)!.settings.arguments as String;
            return CheckoutScreen(orderId: orderId);
          },
          '/order-history': (context) => const OrderListPage(),
        },
      ),
    );
  }
}

class _AuthGate extends StatefulWidget {
  const _AuthGate();

  @override
  State<_AuthGate> createState() => _AuthGateState();
}

class _AuthGateState extends State<_AuthGate> {
  Future<void>? _authCheck;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    _authCheck ??=
        Provider.of<AuthProvider>(context, listen: false).checkAuthStatus();
  }

  @override
  Widget build(BuildContext context) {
    return FutureBuilder<void>(
      future: _authCheck,
      builder: (context, snapshot) {
        if (snapshot.connectionState != ConnectionState.done) {
          return const Scaffold(
            body: Center(child: CircularProgressIndicator()),
          );
        }

        return Consumer<AuthProvider>(
          builder: (context, authProvider, child) {
            return authProvider.isAuthenticated
                ? const MainLayout()
                : const LoginScreen();
          },
        );
      },
    );
  }
}
