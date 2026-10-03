import 'dart:convert';
import 'package:http/http.dart' as http;

/// Serviço de notificações usando OneSignal
///
/// Este serviço envia notificações push via OneSignal REST API
class NotificationService {
  static final NotificationService _instance = NotificationService._internal();
  factory NotificationService() => _instance;
  NotificationService._internal();

  /// Envia uma notificação usando OneSignal
  ///
  /// Envia notificações push para usuários específicos via OneSignal REST API.
  /// Usa External User ID (email) para identificar o destinatário.
  Future<bool> sendNotification({
    required String title,
    required String body,
    String? userEmail,
    Map<String, dynamic>? data,
  }) async {
    try {
      // Credenciais do OneSignal
      const String oneSignalAppId = String.fromEnvironment('ONESIGNAL_APP_ID');
      const String oneSignalApiKey = String.fromEnvironment('ONESIGNAL_REST_API_KEY');

      if (oneSignalAppId.isEmpty || oneSignalApiKey.isEmpty) {
        return false;
      }

      // Preparar o payload da notificação
      final Map<String, dynamic> notification = {
        'app_id': oneSignalAppId,
        'headings': {'en': title},
        'contents': {'en': body},
        if (data != null) 'data': data,
      };

      // Adicionar destino
      if (userEmail != null && userEmail.isNotEmpty) {
        notification['include_external_user_ids'] = [userEmail];
      } else {
        notification['included_segments'] = ['All'];
      }

      // Enviar para OneSignal API
      final response = await http.post(
        Uri.parse('https://onesignal.com/api/v1/notifications'),
        headers: {
          'Content-Type': 'application/json',
          'Authorization': 'Basic $oneSignalApiKey',
        },
        body: jsonEncode(notification),
      );

      return response.statusCode == 200;
    } catch (e) {
      return false;
    }
  }
}
