import 'product.dart';

enum StockStatus {
  Available,
  SoldOut,
  NotAvailable,
}

StockStatus _parseStockStatus(dynamic value) {
  if (value is num) {
    final index = value.toInt();
    if (index >= 0 && index < StockStatus.values.length) {
      return StockStatus.values[index];
    }
  }

  if (value is String) {
    final numericValue = int.tryParse(value);
    if (numericValue != null &&
        numericValue >= 0 &&
        numericValue < StockStatus.values.length) {
      return StockStatus.values[numericValue];
    }

    return StockStatus.values.firstWhere(
      (status) => status.name.toLowerCase() == value.toLowerCase(),
      orElse: () => StockStatus.NotAvailable,
    );
  }

  return StockStatus.NotAvailable;
}

class Stock {
  final String id;
  final String ref;
  final String productId;
  final StockStatus stockStatus;
  final Product? product;

  Stock({
    required this.id,
    required this.ref,
    required this.productId,
    required this.stockStatus,
    this.product,
  });

  factory Stock.fromJson(Map<String, dynamic> json) {
    return Stock(
      id: json['id']?.toString() ?? '',
      ref: json['ref']?.toString() ?? '',
      productId: json['productId']?.toString() ?? '',
      stockStatus: _parseStockStatus(json['stockStatus']),
      product: json['product'] is Map<String, dynamic>
          ? Product.fromJson(json['product'] as Map<String, dynamic>)
          : null,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'ref': ref,
      'productId': productId,
      'stockStatus': stockStatus.index,
      'product': product?.toJson(),
    };
  }
}
