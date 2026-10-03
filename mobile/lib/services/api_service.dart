import 'dart:async';
import 'dart:convert';

import 'package:http/http.dart' as http;

import '../core/constants/api_constants.dart';

class ApiService {
  static final ApiService _instance = ApiService._internal();
  factory ApiService() => _instance;
  ApiService._internal();

  static const Duration _requestTimeout = Duration(seconds: 20);

  String? _token;

  void setToken(String token) {
    _token = token;
  }

  void clearToken() {
    _token = null;
  }

  Map<String, String> _getHeaders() {
    final headers = <String, String>{
      'Content-Type': 'application/json',
      'Accept': 'application/json',
    };

    if (_token != null && _token!.isNotEmpty) {
      headers['Authorization'] = 'Bearer $_token';
    }

    return headers;
  }

  Future<dynamic> get(String endpoint) async {
    return _execute(
      () => http
          .get(
            Uri.parse('${ApiConstants.baseUrl}$endpoint'),
            headers: _getHeaders(),
          )
          .timeout(_requestTimeout),
    );
  }

  Future<dynamic> post(String endpoint, Map<String, dynamic> data) async {
    return _execute(
      () => http
          .post(
            Uri.parse('${ApiConstants.baseUrl}$endpoint'),
            headers: _getHeaders(),
            body: jsonEncode(data),
          )
          .timeout(_requestTimeout),
    );
  }

  Future<dynamic> put(String endpoint, Map<String, dynamic> data) async {
    return _execute(
      () => http
          .put(
            Uri.parse('${ApiConstants.baseUrl}$endpoint'),
            headers: _getHeaders(),
            body: jsonEncode(data),
          )
          .timeout(_requestTimeout),
    );
  }

  Future<dynamic> patch(String endpoint, Map<String, dynamic> data) async {
    return _execute(
      () => http
          .patch(
            Uri.parse('${ApiConstants.baseUrl}$endpoint'),
            headers: _getHeaders(),
            body: jsonEncode(data),
          )
          .timeout(_requestTimeout),
    );
  }

  Future<dynamic> delete(String endpoint) async {
    return _execute(
      () => http
          .delete(
            Uri.parse('${ApiConstants.baseUrl}$endpoint'),
            headers: _getHeaders(),
          )
          .timeout(_requestTimeout),
    );
  }

  Future<dynamic> _execute(Future<http.Response> Function() request) async {
    try {
      final response = await request();
      return _handleResponse(response);
    } on TimeoutException {
      throw Exception('A ligação à API excedeu o tempo limite.');
    } on FormatException {
      throw Exception('A resposta recebida da API não é válida.');
    } catch (e) {
      if (e is Exception) {
        rethrow;
      }
      throw Exception('Não foi possível ligar à API.');
    }
  }

  dynamic _handleResponse(http.Response response) {
    if (response.statusCode >= 200 && response.statusCode < 300) {
      if (response.body.trim().isEmpty) {
        return <String, dynamic>{};
      }

      try {
        return jsonDecode(response.body);
      } on FormatException {
        throw Exception('A resposta recebida da API não é válida.');
      }
    }

    throw Exception(_extractErrorMessage(response));
  }

  String _extractErrorMessage(http.Response response) {
    if (response.statusCode == 401) {
      return 'Sessão inválida ou credenciais incorretas.';
    }
    if (response.statusCode == 403) {
      return 'Não tem permissão para realizar esta operação.';
    }
    if (response.statusCode == 404) {
      return 'O recurso pedido não foi encontrado.';
    }

    if (response.body.trim().isNotEmpty) {
      try {
        final decoded = jsonDecode(response.body);
        if (decoded is Map<String, dynamic>) {
          final message = decoded['message'];
          if (message is String && message.trim().isNotEmpty) {
            return message;
          }

          final errors = decoded['errors'];
          if (errors is List && errors.isNotEmpty) {
            return errors.map((e) => e.toString()).join('\n');
          }
          if (errors is Map && errors.isNotEmpty) {
            final values = <String>[];
            for (final value in errors.values) {
              if (value is List) {
                values.addAll(value.map((e) => e.toString()));
              } else {
                values.add(value.toString());
              }
            }
            if (values.isNotEmpty) {
              return values.join('\n');
            }
          }
        }
      } catch (_) {
        // Não expor o corpo bruto da resposta, que pode conter detalhes internos.
      }
    }

    return 'A API devolveu o erro ${response.statusCode}.';
  }
}
