class ApiConstants {
  //emulador
  static const String baseUrl = String.fromEnvironment(
      'API_BASE_URL',
      defaultValue: 'http://10.0.2.2:5079/api',
    );

  // Endpoints de autenticação
  static const String authLogin = '/Auth/login';
  static const String authRegister = '/UserApi';
  static const String authToken = '/auth/token';

  // Endpoints de cliente
  static String getClientByUserId(String userId) =>
      '/ClientApi/user/$userId'; // + {userId}

  // Endpoints de produtos
  static const String products = '/v1/products/mobile';
  static const String productById = '/v1/products/'; // + {id}
  static const String createProduct = '/v1/products';
  static const String updateProduct = '/v1/products/'; // + {id}
  static const String deleteProduct = '/v1/products/'; // + {id}
  static const String productsByType =
      '/v1/products/products-by-type/'; // + {type:int}
  static const String productsSearch =
      '/v1/products/products-search?searchTerm='; // + {text}
  static const String productsByCategory =
      '/v1/products/category/'; // + {categoryId}
  static const String productsAvailable = '/v1/products/available';
  static const String productsLowStock = '/v1/products/low-stock-products';

  // Endpoints de pedidos (orders)
  static const String orders = '/v1/OrderApi';
  static String orderById(String id) => '/v1/OrderApi/$id';
  static const String createOrder = '/v1/OrderApi';
  static String ordersByClient(String clientId) =>
      '/v1/OrderApi/client/$clientId';
  static String checkout(String orderId) => '/v1/OrderApi/$orderId/checkout';
  static String downloadOrderPdf(String orderId) =>
      '/v1/OrderApi/$orderId/download-pdf';
  static String cancelOrder(String orderId) => '/v1/OrderApi/$orderId/cancel';
  static String completeOrder(String orderId) =>
      '/v1/OrderApi/$orderId/complete';
  static String activeOrder(String orderId) => '/v1/OrderApi/$orderId/active';

  // Endpoints de manutenções
  static const String maintenances = '/maintenances';
  static const String maintenanceById = '/maintenances/'; // + id
  static const String createMaintenance = '/maintenances/create';

  // Endpoints de clientes
  static const String clients = '/clients';
  static const String clientById = '/clients/'; // + id
}
