import 'package:flutter_test/flutter_test.dart';
import 'package:loja_maquinas_mobile/models/stock.dart';

void main() {
  test('StockStatus numérico usa os mesmos índices do backend', () {
    expect(
      Stock.fromJson({'stockStatus': 0}).stockStatus,
      StockStatus.Available,
    );
    expect(
      Stock.fromJson({'stockStatus': 1}).stockStatus,
      StockStatus.SoldOut,
    );
    expect(
      Stock.fromJson({'stockStatus': 2}).stockStatus,
      StockStatus.NotAvailable,
    );
  });

  test('StockStatus continua compatível com nomes em string', () {
    expect(
      Stock.fromJson({'stockStatus': 'Available'}).stockStatus,
      StockStatus.Available,
    );
  });
}
