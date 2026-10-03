import 'package:flutter_test/flutter_test.dart';
import 'package:loja_maquinas_mobile/models/product.dart';
import 'package:loja_maquinas_mobile/models/stock.dart';

void main() {
  group('Product.fromJson', () {
    test('Lê campos básicos com type como int', () {
      final json = {
        'id': 'p1',
        'name': 'maquina1',
        'type': 0, // ProductType.Machine
        'brand': 'brand1',
        'model': 'model1',
        'yearManufacture': 2020,
        'description': 'Máquina potente',
        'price': 5000.0,
        'quantity': 2,
        'image': 'image-url',
        'isActive': true,
        'categoryId': 'cat1',
        'stocks': [], // lista vazia para não chamar Stock.fromJson
      };

      final product = Product.fromJson(json);

      expect(product.id, 'p1');
      expect(product.name, 'maquina1');
      expect(product.type, ProductType.Machine);
      expect(product.brand, 'brand1');
      expect(product.model, 'model1');
      expect(product.yearManufacture, 2020);
      expect(product.description, 'Máquina potente');
      expect(product.price, 5000.0);
      expect(product.quantity, 2);
      expect(product.image, 'image-url');
      expect(product.isActive, true);
      expect(product.categoryId, 'cat1');
      expect(product.stocks, isEmpty);
    });

    test('Consegue interpretar type como String (ex: "part")', () {
      final json = {
        'id': 'p2',
        'name': 'Filtro',
        'type': 'Part',
        'brand': 'brand1',
        'model': 'model1',
        'yearManufacture': 2024,
        'description': 'Peça de substituição',
        'price': 50.0,
        'quantity': 10,
        'image': 'image-url',
        'isActive': false,
        'categoryId': 'cat2',
        'stocks': [],
      };

      final product = Product.fromJson(json);

      expect(product.type, ProductType.Part);
      expect(product.isActive, false);
      expect(product.quantity, 10);
    });

    test('Usa defaults quando campos em falta', () {
      final json = <String, dynamic>{};

      final product = Product.fromJson(json);

      expect(product.id, '');
      expect(product.name, '');
      expect(product.type, ProductType.Part); // default do _parseProductType
      expect(product.brand, '');
      expect(product.model, '');
      expect(product.yearManufacture, 0);
      expect(product.description, '');
      expect(product.price, 0.0);
      expect(product.quantity, 0);
      expect(product.image, '');
      expect(product.isActive, false);
      expect(product.categoryId, '');
      expect(product.stocks, isEmpty);
    });
  });

  group('Product.toJson', () {
    test('Gera JSON com type como index e stocks como lista de mapas', () {
      final product = Product(
        id: 'p3',
        name: 'maquina2',
        type: ProductType.Machine,
        brand: 'brand2',
        model: 'model2',
        yearManufacture: 2023,
        description: 'top',
        price: 999.99,
        quantity: 3,
        image: 'image-url',
        isActive: true,
        categoryId: 'cat3',
        stocks: const <Stock>[], // vazia, não precisamos instanciar Stock
      );

      final json = product.toJson();

      expect(json['id'], 'p3');
      expect(json['name'], 'maquina2');
      expect(json['type'], ProductType.Machine.index);
      expect(json['brand'], 'brand2');
      expect(json['model'], 'model2');
      expect(json['yearManufacture'], 2023);
      expect(json['description'], 'top');
      expect(json['price'], 999.99);
      expect(json['quantity'], 3);
      expect(json['image'], 'image-url');
      expect(json['isActive'], true);
      expect(json['categoryId'], 'cat3');
      expect(json['stocks'], isA<List>());
      expect((json['stocks'] as List).length, 0);
    });
  });
}
