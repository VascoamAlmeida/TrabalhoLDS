import 'package:flutter/foundation.dart';
import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import 'package:jwt_decoder/jwt_decoder.dart';
import 'package:onesignal_flutter/onesignal_flutter.dart';

import '../core/constants/api_constants.dart';
import '../models/user.dart';
import '../services/api_service.dart';

class AuthProvider with ChangeNotifier {
  final ApiService _apiService = ApiService();
  final FlutterSecureStorage _storage = const FlutterSecureStorage();

  User? _user;
  bool _isLoading = false;
  bool _isInitialized = false;
  String? _errorMessage;

  User? get user => _user;
  bool get isLoading => _isLoading;
  bool get isInitialized => _isInitialized;
  String? get errorMessage => _errorMessage;
  bool get isAuthenticated => _user != null;

  Future<bool> login(String email, String password) async {
    try {
      _isLoading = true;
      _errorMessage = null;
      notifyListeners();

      final response = await _apiService.post(ApiConstants.authLogin, {
        'email': email,
        'password': password,
      });

      final token = response is Map<String, dynamic>
          ? response['token']?.toString()
          : null;

      if (token == null || token.isEmpty || JwtDecoder.isExpired(token)) {
        _errorMessage = 'A API não devolveu uma sessão válida.';
        return false;
      }

      final decodedToken = JwtDecoder.decode(token);
      final userId = (decodedToken['userId'] ?? decodedToken['nameid'])
          ?.toString();
      final tokenEmail = decodedToken['email']?.toString() ?? email;

      if (userId == null || userId.isEmpty) {
        _errorMessage = 'O token recebido não contém o identificador do utilizador.';
        return false;
      }

      _user = User(
        id: userId,
        email: tokenEmail,
        name: decodedToken['name']?.toString() ??
            decodedToken['unique_name']?.toString() ??
            tokenEmail,
      );

      _apiService.setToken(token);
      await _persistSession(token, _user!);

      if (!kIsWeb) {
        // Usar o ID interno em vez do email evita usar PII como External User ID.
        OneSignal.login(_user!.id);
      }

      return true;
    } catch (e) {
      _errorMessage = _cleanExceptionMessage(e);
      return false;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<bool> register(String email, String password) async {
    try {
      _isLoading = true;
      _errorMessage = null;
      notifyListeners();

      final response = await _apiService.post(ApiConstants.authRegister, {
        'email': email,
        'password': password,
        'userType': 'CLIENT',
        'isActive': true,
      });

      final success = response is Map<String, dynamic> &&
          response['success'] == true;

      if (!success) {
        _errorMessage = response is Map<String, dynamic>
            ? response['message']?.toString() ?? 'Erro ao registar.'
            : 'Erro ao registar.';
      }

      return success;
    } catch (e) {
      _errorMessage = _cleanExceptionMessage(e);
      return false;
    } finally {
      _isLoading = false;
      notifyListeners();
    }
  }

  Future<void> logout() async {
    _user = null;
    _apiService.clearToken();

    try {
      await _storage.delete(key: 'auth_token');
      await _storage.delete(key: 'user_id');
      await _storage.delete(key: 'user_email');
      await _storage.delete(key: 'user_name');
    } catch (_) {
      // O estado em memória já foi limpo. Falhas de storage não devem bloquear logout.
    }

    if (!kIsWeb) {
      try {
        OneSignal.logout();
      } catch (_) {
        // OneSignal pode não estar configurado em ambientes de teste/desenvolvimento.
      }
    }

    notifyListeners();
  }

  Future<void> checkAuthStatus() async {
    if (_isInitialized) {
      return;
    }

    try {
      final token = await _storage.read(key: 'auth_token');
      if (token == null || token.isEmpty || JwtDecoder.isExpired(token)) {
        _user = null;
        _apiService.clearToken();
        return;
      }

      final decodedToken = JwtDecoder.decode(token);
      final tokenUserId =
          (decodedToken['userId'] ?? decodedToken['nameid'])?.toString();
      final tokenEmail = decodedToken['email']?.toString();

      final storedUserId = await _storage.read(key: 'user_id');
      final storedEmail = await _storage.read(key: 'user_email');
      final storedName = await _storage.read(key: 'user_name');

      final userId = tokenUserId ?? storedUserId;
      final email = tokenEmail ?? storedEmail;

      if (userId == null || userId.isEmpty || email == null || email.isEmpty) {
        _user = null;
        _apiService.clearToken();
        return;
      }

      _apiService.setToken(token);
      _user = User(
        id: userId,
        email: email,
        name: storedName ?? email,
      );

      if (!kIsWeb) {
        try {
          OneSignal.login(userId);
        } catch (_) {
          // OneSignal é opcional na configuração pública do projeto.
        }
      }
    } catch (_) {
      // Permite que widget tests ou plataformas sem secure storage iniciem no login.
      _user = null;
      _apiService.clearToken();
    } finally {
      _isInitialized = true;
      notifyListeners();
    }
  }

  Future<void> _persistSession(String token, User user) async {
    await _storage.write(key: 'auth_token', value: token);
    await _storage.write(key: 'user_id', value: user.id);
    await _storage.write(key: 'user_email', value: user.email);
    await _storage.write(key: 'user_name', value: user.name);
  }

  String _cleanExceptionMessage(Object error) {
    final message = error.toString();
    return message.startsWith('Exception: ')
        ? message.substring('Exception: '.length)
        : message;
  }
}
