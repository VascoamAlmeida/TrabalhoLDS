enum OrderType {
  sale,
  rent,
}

enum OrderStatus {
  pending,
  active,
  completed,
  cancelled,
}

const List<String> priceRanges = <String>[
  'all',
  '<1000',
  '1000-3000',
  '3000-5000',
  '5000-7000',
  '7000+',
];

OrderType _parseOrderType(dynamic value) {
  if (value is num) {
    final index = value.toInt();
    if (index >= 0 && index < OrderType.values.length) {
      return OrderType.values[index];
    }
  }

  if (value is String) {
    final numericValue = int.tryParse(value);
    if (numericValue != null &&
        numericValue >= 0 &&
        numericValue < OrderType.values.length) {
      return OrderType.values[numericValue];
    }

    return OrderType.values.firstWhere(
      (type) => type.name.toLowerCase() == value.toLowerCase(),
      orElse: () => OrderType.sale,
    );
  }

  return OrderType.sale;
}

OrderStatus _parseOrderStatus(dynamic value) {
  if (value is num) {
    final index = value.toInt();
    if (index >= 0 && index < OrderStatus.values.length) {
      return OrderStatus.values[index];
    }
  }

  if (value is String) {
    final numericValue = int.tryParse(value);
    if (numericValue != null &&
        numericValue >= 0 &&
        numericValue < OrderStatus.values.length) {
      return OrderStatus.values[numericValue];
    }

    return OrderStatus.values.firstWhere(
      (status) => status.name.toLowerCase() == value.toLowerCase(),
      orElse: () => OrderStatus.pending,
    );
  }

  return OrderStatus.pending;
}

class Order {
  final String id;
  final String clientId;
  final String stockId;
  final DateTime orderDate;
  final double total;
  final OrderType type;
  final OrderStatus status;

  Order({
    required this.id,
    required this.clientId,
    required this.stockId,
    required this.orderDate,
    required this.total,
    required this.type,
    required this.status,
  });

  factory Order.fromJson(Map<String, dynamic> json) {
    final rawDate = json['orderDate']?.toString();

    return Order(
      id: json['id']?.toString() ?? '',
      clientId: json['clientId']?.toString() ?? '',
      stockId: json['stockId']?.toString() ?? '',
      orderDate: rawDate == null
          ? DateTime.now()
          : DateTime.tryParse(rawDate) ?? DateTime.now(),
      total: (json['total'] as num?)?.toDouble() ??
          double.tryParse(json['total']?.toString() ?? '') ??
          0.0,
      type: _parseOrderType(json['type']),
      status: _parseOrderStatus(json['status']),
    );
  }

  Order copyWith({
    String? id,
    String? clientId,
    String? stockId,
    DateTime? orderDate,
    double? total,
    OrderType? type,
    OrderStatus? status,
  }) {
    return Order(
      id: id ?? this.id,
      clientId: clientId ?? this.clientId,
      stockId: stockId ?? this.stockId,
      orderDate: orderDate ?? this.orderDate,
      total: total ?? this.total,
      type: type ?? this.type,
      status: status ?? this.status,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'clientId': clientId,
      'stockId': stockId,
      'orderDate': orderDate.toIso8601String(),
      'total': total,
      'type': type.index,
      'status': status.index,
    };
  }
}
