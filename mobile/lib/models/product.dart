import 'stock.dart';

enum ProductType {
  Machine,
  Part,
}

ProductType _parseProductType(dynamic value) {
  if (value is num) {
    final index = value.toInt();
    if (index >= 0 && index < ProductType.values.length) {
      return ProductType.values[index];
    }
    return ProductType.Part;
  } else if (value is String) {
    return ProductType.values.firstWhere(
      (e) => e.toString().split('.').last.toLowerCase() == value.toLowerCase(),
      orElse: () => ProductType.Part,
    );
  }
  return ProductType.Part;
}

class Product {
  final String id;
  final String name;
  final ProductType type;
  final String brand;
  final String model;
  final int yearManufacture;
  final String description;
  final double price;
  final int quantity;
  final String image;
  final bool isActive;
  final String categoryId;
  final List<Stock> stocks;

  Product({
    required this.id,
    required this.name,
    required this.type,
    required this.brand,
    required this.model,
    required this.yearManufacture,
    required this.description,
    required this.price,
    required this.quantity,
    required this.image,
    required this.isActive,
    required this.categoryId,
    required this.stocks,
  });

  factory Product.fromJson(Map<String, dynamic> json) {
    final List<dynamic> stocksJson = json['stocks'] ?? [];

    return Product(
      id: json['id']?.toString() ?? '',
      name: json['name'] ?? '',
      type: _parseProductType(json['type']),
      brand: json['brand'] ?? '',
      model: json['model'] ?? '',
      yearManufacture: (json['yearManufacture'] ?? 0).toInt(),
      description: json['description'] ?? '',
      price: (json['price'] ?? 0).toDouble(),
      quantity: (json['quantity'] ?? 0).toInt(),
      image: json['image'] ?? '',
      isActive: json['isActive'] ?? false,
      categoryId: json['categoryId']?.toString() ?? '',
      stocks: stocksJson
          .map<Stock>((s) => Stock.fromJson(s as Map<String, dynamic>))
          .toList(),
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'name': name,
      'type': type.index,
      'brand': brand,
      'model': model,
      'yearManufacture': yearManufacture,
      'description': description,
      'price': price,
      'quantity': quantity,
      'image': image,
      'isActive': isActive,
      'categoryId': categoryId,
      'stocks': stocks.map((s) => s.toJson()).toList(),
    };
  }
}
