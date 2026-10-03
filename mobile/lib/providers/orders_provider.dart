import 'package:flutter/foundation.dart';

import '../core/constants/api_constants.dart';
import '../models/order.dart';
import '../services/api_service.dart';

class OrdersProvider with ChangeNotifier {
  final ApiService _apiService = ApiService();

  List<Order> _orders = [];
  Order? _selectedOrder;
  bool _isLoading = false;
  String? _errorMessage;

  List<Order> get orders => List.unmodifiable(_orders);
  Order? get selectedOrder => _selectedOrder;
  bool get isLoading => _isLoading;
  String? get errorMessage => _errorMessage;

  Future<bool> fetchOrderById(String id) async {
    return _loadSingleOrder(ApiConstants.orderById(id), 'Erro ao carregar pedido');
  }

  Future<bool> fetchOrderForCheckout(String id) async {
    return _loadSingleOrder(
      ApiConstants.checkout(id),
      'Erro ao carregar dados do checkout',
    );
  }

  Future<bool> _loadSingleOrder(String endpoint, String errorMessage) async {
    try {
      _setLoading(true);
      final response = await _apiService.get(endpoint);
      if (response is Map<String, dynamic>) {
        _selectedOrder = Order.fromJson(response);
        return true;
      }

      _errorMessage = errorMessage;
      return false;
    } catch (e) {
      _errorMessage = _cleanExceptionMessage(e);
      return false;
    } finally {
      _setLoading(false);
    }
  }

  Future<bool> fetchOrdersByClient(String clientId) async {
    try {
      _setLoading(true);
      final response = await _apiService.get(ApiConstants.ordersByClient(clientId));

      if (response is List) {
        _orders = response
            .whereType<Map<String, dynamic>>()
            .map(Order.fromJson)
            .toList();
        return true;
      }

      _orders = [];
      _errorMessage = 'Erro ao carregar pedidos do cliente';
      return false;
    } catch (e) {
      _orders = [];
      _errorMessage = _cleanExceptionMessage(e);
      return false;
    } finally {
      _setLoading(false);
    }
  }

  Future<String?> createOrder(Map<String, dynamic> orderData) async {
    try {
      _setLoading(true);
      final response = await _apiService.post(
        ApiConstants.createOrder,
        orderData,
      );

      if (response is Map<String, dynamic> && response['success'] == true) {
        final orderId = response['orderId']?.toString();
        final clientId = (orderData['clientId'] ?? orderData['ClientId'])?.toString();

        if (clientId != null && clientId.isNotEmpty) {
          await fetchOrdersByClient(clientId);
        }

        return orderId;
      }

      _errorMessage = response is Map<String, dynamic>
          ? response['message']?.toString() ?? 'Erro ao criar pedido'
          : 'Erro ao criar pedido';
      return null;
    } catch (e) {
      _errorMessage = _cleanExceptionMessage(e);
      return null;
    } finally {
      _setLoading(false);
    }
  }

  Future<bool> cancelOrder(String orderId) async {
    return _changeStatus(
      orderId,
      ApiConstants.cancelOrder(orderId),
      OrderStatus.cancelled,
      'Erro ao cancelar pedido',
    );
  }

  Future<bool> completeOrder(String orderId) async {
    return _changeStatus(
      orderId,
      ApiConstants.completeOrder(orderId),
      OrderStatus.completed,
      'Erro ao completar pedido',
    );
  }

  Future<bool> activeOrder(String orderId) async {
    final success = await _changeStatus(
      orderId,
      ApiConstants.activeOrder(orderId),
      OrderStatus.active,
      'Erro ao confirmar pedido',
    );

    // O backend acrescenta IVA quando passa o pedido para Active.
    if (success) {
      _replaceOrder(orderId, (order) => order.copyWith(total: order.total * 1.23));
    }
    return success;
  }

  Future<bool> _changeStatus(
    String orderId,
    String endpoint,
    OrderStatus status,
    String fallbackError,
  ) async {
    try {
      _setLoading(true);
      final response = await _apiService.patch(endpoint, <String, dynamic>{});

      if (response is Map<String, dynamic> && response['success'] == false) {
        _errorMessage = response['message']?.toString() ?? fallbackError;
        return false;
      }

      _replaceOrder(orderId, (order) => order.copyWith(status: status));
      return true;
    } catch (e) {
      _errorMessage = _cleanExceptionMessage(e);
      return false;
    } finally {
      _setLoading(false);
    }
  }

  void _replaceOrder(String orderId, Order Function(Order order) update) {
    final index = _orders.indexWhere((order) => order.id == orderId);
    if (index >= 0) {
      _orders[index] = update(_orders[index]);
    }

    if (_selectedOrder?.id == orderId) {
      _selectedOrder = update(_selectedOrder!);
    }
    notifyListeners();
  }

  String getDownloadPdfUrl(String orderId) {
    return '${ApiConstants.baseUrl}${ApiConstants.downloadOrderPdf(orderId)}';
  }

  void clearSelectedOrder() {
    _selectedOrder = null;
    notifyListeners();
  }

  void _setLoading(bool value) {
    _isLoading = value;
    if (value) {
      _errorMessage = null;
    }
    notifyListeners();
  }

  String _cleanExceptionMessage(Object error) {
    final message = error.toString();
    return message.startsWith('Exception: ')
        ? message.substring('Exception: '.length)
        : message;
  }
}
