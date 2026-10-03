import 'package:flutter/foundation.dart';

import '../core/constants/api_constants.dart';
import '../models/client.dart';
import '../services/api_service.dart';

class ClientProvider with ChangeNotifier {
  final ApiService _api = ApiService();

  Client? _selectedClient;
  bool _isLoading = false;
  String? _errorMessage;

  Client? get selectedClient => _selectedClient;
  bool get isLoading => _isLoading;
  String? get errorMessage => _errorMessage;

  Future<Client?> fetchClientByUserId(String userId) async {
    try {
      _isLoading = true;
      _errorMessage = null;
      notifyListeners();

      final response = await _api.get(ApiConstants.getClientByUserId(userId));
      if (response is! Map<String, dynamic>) {
        _selectedClient = null;
        _errorMessage = 'Resposta de cliente inválida.';
        return null;
      }

      _selectedClient = Client.fromJson(response);
      return _selectedClient;
    } catch (e) {
      _selectedClient = null;
      final message = e.toString();
      _errorMessage = message.startsWith('Exception: ')
          ? message.substring('Exception: '.length)
          : message;
      return null;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }
}
