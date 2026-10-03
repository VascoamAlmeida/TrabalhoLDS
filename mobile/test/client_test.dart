import 'package:flutter_test/flutter_test.dart';
import 'package:loja_maquinas_mobile/models/client.dart';

void main() {
  group('Client.fromJson', () {
    test('preserva código postal e NIF como strings', () {
      final client = Client.fromJson({
        'id': 'c1',
        'userId': 'u1',
        'firstName': 'João',
        'lastName': 'Silva',
        'address': 'Rua A',
        'postalCode': '0123-456',
        'phoneNumber': '912345678',
        'nif': '123456789',
      });

      expect(client.id, 'c1');
      expect(client.userId, 'u1');
      expect(client.postalCode, '0123-456');
      expect(client.phoneNumber, '912345678');
      expect(client.nif, '123456789');
    });

    test('converte valores simples para string sem perder o contrato do modelo', () {
      final client = Client.fromJson({
        'id': 10,
        'userId': 20,
        'postalCode': 4000,
        'phoneNumber': 987654321,
        'nif': 987654321,
      });

      expect(client.id, '10');
      expect(client.userId, '20');
      expect(client.postalCode, '4000');
      expect(client.phoneNumber, '987654321');
      expect(client.nif, '987654321');
    });

    test('usa strings vazias quando campos estão em falta', () {
      final client = Client.fromJson(<String, dynamic>{});

      expect(client.id, '');
      expect(client.userId, '');
      expect(client.postalCode, '');
      expect(client.phoneNumber, '');
      expect(client.nif, '');
    });
  });

  test('Client.toJson mantém os formatos enviados pela API', () {
    final client = Client(
      id: 'c2',
      userId: 'u2',
      firstName: 'Ana',
      lastName: 'Ferreira',
      address: 'Rua C',
      postalCode: '5000-123',
      phoneNumber: '900000000',
      nif: '222333444',
    );

    final json = client.toJson();

    expect(json['postalCode'], '5000-123');
    expect(json['phoneNumber'], '900000000');
    expect(json['nif'], '222333444');
  });
}
