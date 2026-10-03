import 'package:flutter/foundation.dart';

import '../core/constants/api_constants.dart';
import '../models/product.dart';
import '../services/api_service.dart';

class ProductsProvider with ChangeNotifier {
  final ApiService _apiService = ApiService();

  List<Product> _products = [];
  Product? _selectedProduct;
  bool _isLoading = false;
  String? _errorMessage;

  List<Product> get products => List.unmodifiable(_products);
  Product? get selectedProduct => _selectedProduct;
  bool get isLoading => _isLoading;
  String? get errorMessage => _errorMessage;

  Future<bool> fetchProducts() => _loadList(ApiConstants.products);

  Future<bool> fetchProductById(String id) async {
    try {
      _setLoading(true);
      final response = await _apiService.get('${ApiConstants.productById}$id');
      if (response is Map<String, dynamic>) {
        _selectedProduct = Product.fromJson(response);
        return true;
      }
      _errorMessage = 'Produto não encontrado';
      return false;
    } catch (e) {
      _errorMessage = _cleanExceptionMessage(e);
      return false;
    } finally {
      _setLoading(false);
    }
  }

  Future<bool> searchProducts(String term) {
    final encodedTerm = Uri.encodeQueryComponent(term.trim());
    return _loadList('${ApiConstants.productsSearch}$encodedTerm');
  }

  Future<bool> fetchProductsByType(int type) =>
      _loadList('${ApiConstants.productsByType}$type');

  Future<bool> fetchProductsByCategory(String categoryId) =>
      _loadList('${ApiConstants.productsByCategory}$categoryId');

  Future<bool> fetchAvailableProducts() =>
      _loadList(ApiConstants.productsAvailable);

  Future<bool> fetchLowStockProducts() =>
      _loadList(ApiConstants.productsLowStock);

  Future<bool> _loadList(String endpoint) async {
    try {
      _setLoading(true);
      final response = await _apiService.get(endpoint);
      if (response is List) {
        _products = response
            .whereType<Map<String, dynamic>>()
            .map(Product.fromJson)
            .toList();
        return true;
      }

      _products = [];
      _errorMessage = 'Erro ao carregar produtos';
      return false;
    } catch (e) {
      _products = [];
      _errorMessage = _cleanExceptionMessage(e);
      return false;
    } finally {
      _setLoading(false);
    }
  }

  void clearSelectedProduct() {
    _selectedProduct = null;
    notifyListeners();
  }

  void _setLoading(bool value) {
    _isLoading = value;
    if (value) _errorMessage = null;
    notifyListeners();
  }

  String _cleanExceptionMessage(Object error) {
    final message = error.toString();
    return message.startsWith('Exception: ')
        ? message.substring('Exception: '.length)
        : message;
  }
}
