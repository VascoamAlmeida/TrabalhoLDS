import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../models/client.dart';
import '../../models/order.dart';
import '../../models/product.dart';
import '../../models/stock.dart';
import '../../providers/auth_provider.dart';
import '../../providers/client_provider.dart';
import '../../providers/orders_provider.dart';

class CreateOrderScreen extends StatefulWidget {
  final Stock stock;
  final Product product;

  const CreateOrderScreen({
    super.key,
    required this.stock,
    required this.product,
  });

  @override
  State<CreateOrderScreen> createState() => _CreateOrderScreenState();
}

class _CreateOrderScreenState extends State<CreateOrderScreen> {
  bool _isSubmitting = false;
  bool _isLoadingClient = true;
  bool _clientRequested = false;
  Client? _client;

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (!_clientRequested) {
      _clientRequested = true;
      _loadClient();
    }
  }

  Future<void> _loadClient() async {
    final authProvider = Provider.of<AuthProvider>(context, listen: false);
    final userId = authProvider.user?.id;

    if (userId == null || userId.isEmpty) {
      if (mounted) {
        setState(() => _isLoadingClient = false);
      }
      return;
    }

    final clientProvider = Provider.of<ClientProvider>(context, listen: false);
    final result = await clientProvider.fetchClientByUserId(userId);

    if (!mounted) return;
    setState(() {
      _client = result;
      _isLoadingClient = false;
    });
  }

  Future<void> _submitOrder() async {
    if (_client == null) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text(
            'Não existe um perfil de cliente associado à sua conta. '
            'Complete primeiro o perfil na aplicação Web.',
          ),
        ),
      );
      return;
    }

    setState(() => _isSubmitting = true);

    final ordersProvider = Provider.of<OrdersProvider>(context, listen: false);
    final orderData = <String, dynamic>{
      'type': OrderType.sale.index,
      'stockId': widget.stock.id,
      'clientId': _client!.id,
      'status': OrderStatus.pending.index,
      'orderDate': DateTime.now().toIso8601String(),
      'total': widget.product.price,
    };

    final orderId = await ordersProvider.createOrder(orderData);

    if (!mounted) return;
    setState(() => _isSubmitting = false);

    if (orderId == null || orderId.isEmpty) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(ordersProvider.errorMessage ?? 'Erro ao criar pedido'),
          backgroundColor: Colors.red,
        ),
      );
      return;
    }

    Navigator.pushNamed(context, '/checkout', arguments: orderId);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Criar Pedido')),
      body: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            const Text(
              'Produto',
              style: TextStyle(fontWeight: FontWeight.bold),
            ),
            const SizedBox(height: 4),
            TextFormField(
              initialValue: widget.product.name,
              enabled: false,
              decoration: const InputDecoration(border: OutlineInputBorder()),
            ),
            const SizedBox(height: 16),
            Text(
              'Preço base: €${widget.product.price.toStringAsFixed(2)}',
              style: const TextStyle(fontSize: 16),
            ),
            const SizedBox(height: 8),
            if (_isLoadingClient)
              const LinearProgressIndicator()
            else if (_client != null)
              Text(
                'Cliente: ${_client!.firstName} ${_client!.lastName}',
                style: const TextStyle(color: Colors.green),
              )
            else
              const Text(
                'Perfil de cliente em falta. Complete o perfil na aplicação Web antes de efetuar um pedido.',
                style: TextStyle(color: Colors.red),
              ),
            const Spacer(),
            Row(
              children: [
                Expanded(
                  child: ElevatedButton.icon(
                    icon: _isSubmitting
                        ? const SizedBox(
                            width: 18,
                            height: 18,
                            child: CircularProgressIndicator(
                              color: Colors.white,
                              strokeWidth: 2,
                            ),
                          )
                        : const Icon(Icons.check),
                    label: const Text('Confirmar Pedido'),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.green,
                    ),
                    onPressed: _isSubmitting || _isLoadingClient
                        ? null
                        : _submitOrder,
                  ),
                ),
                const SizedBox(width: 12),
                OutlinedButton(
                  onPressed: _isSubmitting ? null : () => Navigator.pop(context),
                  child: const Text('Cancelar'),
                ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
