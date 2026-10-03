import 'package:flutter_test/flutter_test.dart';
import 'package:loja_maquinas_mobile/models/order.dart';

void main() {
  group('Order.fromJson', () {
    test('interpreta enums numéricos devolvidos pelo ASP.NET Core', () {
      final order = Order.fromJson({
        'id': 'o1',
        'clientId': 'c1',
        'stockId': 's1',
        'orderDate': '2025-12-03T23:25:46.7763328',
        'total': 5000.0,
        'type': 1,
        'status': 2,
      });

      expect(order.type, OrderType.rent);
      expect(order.status, OrderStatus.completed);
      expect(order.total, 5000.0);
    });

    test('continua compatível com enums em string', () {
      final order = Order.fromJson({
        'type': 'sale',
        'status': 'active',
        'total': 2500,
      });

      expect(order.type, OrderType.sale);
      expect(order.status, OrderStatus.active);
      expect(order.total, 2500.0);
    });

    test('usa defaults seguros para enums desconhecidos', () {
      final order = Order.fromJson({
        'type': 99,
        'status': 'desconhecido',
      });

      expect(order.type, OrderType.sale);
      expect(order.status, OrderStatus.pending);
    });
  });

  test('Order.toJson envia enums como índices compatíveis com a API', () {
    final order = Order(
      id: 'o10',
      clientId: 'c10',
      stockId: 's10',
      orderDate: DateTime.utc(2025, 1, 1, 12),
      total: 1234.5,
      type: OrderType.rent,
      status: OrderStatus.cancelled,
    );

    final json = order.toJson();

    expect(json['type'], 1);
    expect(json['status'], 3);
  });
}
