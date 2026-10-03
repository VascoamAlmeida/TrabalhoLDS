class Client {
  final String id;
  final String userId;
  final String firstName;
  final String lastName;
  final String address;
  final String postalCode;
  final String phoneNumber;
  final String nif;

  Client({
    required this.id,
    required this.userId,
    required this.firstName,
    required this.lastName,
    required this.address,
    required this.postalCode,
    required this.phoneNumber,
    required this.nif,
  });

  factory Client.fromJson(Map<String, dynamic> json) {
    return Client(
      id: json['id']?.toString() ?? '',
      userId: json['userId']?.toString() ?? '',
      firstName: json['firstName']?.toString() ?? '',
      lastName: json['lastName']?.toString() ?? '',
      address: json['address']?.toString() ?? '',
      postalCode: json['postalCode']?.toString() ?? '',
      phoneNumber: json['phoneNumber']?.toString() ?? '',
      nif: json['nif']?.toString() ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'userId': userId,
      'firstName': firstName,
      'lastName': lastName,
      'address': address,
      'postalCode': postalCode,
      'phoneNumber': phoneNumber,
      'nif': nif,
    };
  }
}
