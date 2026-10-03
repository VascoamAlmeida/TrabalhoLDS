import 'package:flutter/material.dart'; // vai buscar os widgets para o código
import '../models/order.dart';

import 'package:flutter/foundation.dart';
import '../services/api_service.dart';
import '../core/constants/api_constants.dart';

/// Serve para ir buscar a lista de orders na API
class OrderlistProvider with ChangeNotifier {
  final ApiService _api = ApiService();

  /// Lista de encomendas carregadas da API.
  List<Order> _orders = [];

  /// Encomenda atualmente selecionada
  Order? _selectedOrder;

  /// Indica se está a decorrer uma operação de carregamento.
  bool _isLoading = false;

  /// Mensagem de erro da última operação
  String? _errorMessage;

  List<Order> get orders => _orders;
  Order? get selectedOrder => _selectedOrder;
  bool get isLoading => _isLoading;
  String? get errorMessage => _errorMessage;

  ///Faz pedido API das encomendas
  Future<List<Order>> fetchOrdersByClient(String clientId) async {
    try {
      _isLoading = true;
      _errorMessage = null;
      notifyListeners();

      /// Metodo que vai buscar
      final response = await _api.get(ApiConstants.ordersByClient(clientId));
      debugPrint('Resposta ordersByClient: $response');

      if (response != null) {
        ///Converte JSON em instancias
        _orders =
            (response as List).map((json) => Order.fromJson(json)).toList();
      } else {
        _orders = [];
      }

      _isLoading = false;
      notifyListeners();
      return _orders;
    } catch (e) {
      _isLoading = false;
      _errorMessage = e.toString();
      notifyListeners();
      return [];
    }
  }
}
