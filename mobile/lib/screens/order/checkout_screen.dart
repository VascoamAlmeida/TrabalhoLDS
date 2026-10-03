import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../models/client.dart';
import '../../models/order.dart';
import '../../models/user.dart';
import '../../providers/auth_provider.dart';
import '../../providers/client_provider.dart';
import '../../providers/orders_provider.dart';

class CheckoutScreen extends StatefulWidget {
  final String orderId;

  const CheckoutScreen({super.key, required this.orderId});

  @override
  State<CheckoutScreen> createState() => _CheckoutScreenState();
}

class _CheckoutScreenState extends State<CheckoutScreen> {
  bool _isLoading = true;
  Order? _order;
  User? _user;
  Client? _client;

  @override
  void initState() {
    super.initState();
    WidgetsBinding.instance.addPostFrameCallback((_) => _loadOrderDetails());
  }

  Future<void> _loadOrderDetails() async {
    final ordersProvider = Provider.of<OrdersProvider>(context, listen: false);
    final authProvider = Provider.of<AuthProvider>(context, listen: false);
    final clientProvider = Provider.of<ClientProvider>(context, listen: false);

    _user = authProvider.user;

    final orderLoaded =
        await ordersProvider.fetchOrderForCheckout(widget.orderId);

    Client? client;
    final userId = _user?.id;
    if (userId != null && userId.isNotEmpty) {
      client = await clientProvider.fetchClientByUserId(userId);
    }

    if (!mounted) return;

    setState(() {
      _order = orderLoaded ? ordersProvider.selectedOrder : null;
      _client = client;
      _isLoading = false;
    });

    if (!orderLoaded) {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            ordersProvider.errorMessage ?? 'Erro ao carregar pedido',
          ),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  Future<void> _handleCheckout() async {
    final confirmed = await showDialog<bool>(
      context: context,
      builder: (context) => AlertDialog(
        title: const Text('Confirmar Compra'),
        content: const Text('Deseja confirmar este pedido de compra?'),
        actions: [
          TextButton(
            onPressed: () => Navigator.pop(context, false),
            child: const Text('Cancelar'),
          ),
          ElevatedButton(
            onPressed: () => Navigator.pop(context, true),
            child: const Text('Confirmar'),
          ),
        ],
      ),
    );

    if (confirmed != true || !mounted) return;

    final ordersProvider = Provider.of<OrdersProvider>(context, listen: false);
    final success = await ordersProvider.activeOrder(widget.orderId);

    if (!mounted) return;

    if (success) {
      ScaffoldMessenger.of(context).showSnackBar(
        const SnackBar(
          content: Text('Pedido confirmado com sucesso!'),
          backgroundColor: Colors.green,
        ),
      );
      Navigator.pushNamedAndRemoveUntil(context, '/home', (route) => false);
    } else {
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(
            ordersProvider.errorMessage ?? 'Erro ao confirmar pedido',
          ),
          backgroundColor: Colors.red,
        ),
      );
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: const Color(0xFFf8f9fa),
      appBar: AppBar(title: const Text('Finalizar Compra')),
      body: _isLoading
          ? const Center(child: CircularProgressIndicator())
          : _order == null
              ? const Center(child: Text('Pedido não encontrado'))
              : _buildCheckoutContent(context),
    );
  }

  Widget _buildCheckoutContent(BuildContext context) {
    final order = _order!;
    final vat = order.total * 0.23;
    final finalTotal = order.total + vat;

    return SingleChildScrollView(
      padding: const EdgeInsets.all(16),
      child: Column(
        crossAxisAlignment: CrossAxisAlignment.stretch,
        children: [
          _buildSectionCard(
            title: 'Dados do Cliente',
            icon: Icons.person_outline,
            child: _buildClientInfo(),
          ),
          const SizedBox(height: 16),
          _buildSectionCard(
            title: 'Detalhes do Produto',
            icon: Icons.inventory_2,
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Text(
                  'Stock: ${order.stockId}',
                  style: const TextStyle(fontWeight: FontWeight.bold),
                ),
                const SizedBox(height: 8),
                Text(
                  'Tipo: ${order.type == OrderType.sale ? 'Venda' : 'Aluguer'}',
                ),
                const SizedBox(height: 4),
                Text(
                  'Data: ${order.orderDate.day.toString().padLeft(2, '0')}/'
                  '${order.orderDate.month.toString().padLeft(2, '0')}/'
                  '${order.orderDate.year}',
                ),
              ],
            ),
          ),
          const SizedBox(height: 16),
          _buildSectionCard(
            title: 'Resumo do Pedido',
            icon: Icons.receipt_long,
            child: Column(
              children: [
                _buildSummaryRow('Subtotal', order.total),
                const Divider(),
                _buildSummaryRow('Envio', 0, isFree: true),
                const Divider(),
                _buildSummaryRow('IVA (23%)', vat),
                const Divider(thickness: 2),
                _buildSummaryRow('Total', finalTotal, isTotal: true),
              ],
            ),
          ),
          const SizedBox(height: 24),
          Consumer<OrdersProvider>(
            builder: (context, ordersProvider, child) {
              return ElevatedButton.icon(
                onPressed: ordersProvider.isLoading ? null : _handleCheckout,
                icon: ordersProvider.isLoading
                    ? const SizedBox(
                        width: 18,
                        height: 18,
                        child: CircularProgressIndicator(
                          color: Colors.white,
                          strokeWidth: 2,
                        ),
                      )
                    : const Icon(Icons.check_circle_outline),
                label: const Text('Confirmar Compra'),
                style: ElevatedButton.styleFrom(
                  padding: const EdgeInsets.symmetric(vertical: 16),
                  backgroundColor: Colors.green,
                ),
              );
            },
          ),
        ],
      ),
    );
  }

  Widget _buildClientInfo() {
    if (_client != null) {
      final fullName = '${_client!.firstName} ${_client!.lastName}'.trim();
      return Column(
        crossAxisAlignment: CrossAxisAlignment.start,
        children: [
          Text(
            fullName.isEmpty ? 'Cliente' : fullName,
            style: const TextStyle(fontWeight: FontWeight.bold),
          ),
          if (_client!.address.isNotEmpty) ...[
            const SizedBox(height: 6),
            Text(_client!.address),
          ],
          if (_client!.postalCode.isNotEmpty) Text(_client!.postalCode),
          if (_client!.phoneNumber.isNotEmpty) ...[
            const SizedBox(height: 6),
            Text('Telefone: ${_client!.phoneNumber}'),
          ],
          if (_user?.email.isNotEmpty == true) Text(_user!.email),
        ],
      );
    }

    return Text(_user?.email ?? 'Dados de cliente indisponíveis');
  }

  Widget _buildSectionCard({
    required String title,
    required IconData icon,
    required Widget child,
  }) {
    return Card(
      elevation: 2,
      child: Padding(
        padding: const EdgeInsets.all(16),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Row(
              children: [
                Icon(icon, color: Theme.of(context).colorScheme.primary),
                const SizedBox(width: 8),
                Text(
                  title,
                  style: const TextStyle(
                    fontSize: 17,
                    fontWeight: FontWeight.bold,
                  ),
                ),
              ],
            ),
            const SizedBox(height: 14),
            child,
          ],
        ),
      ),
    );
  }

  Widget _buildSummaryRow(
    String label,
    double value, {
    bool isTotal = false,
    bool isFree = false,
  }) {
    final style = TextStyle(
      fontSize: isTotal ? 18 : 15,
      fontWeight: isTotal ? FontWeight.bold : FontWeight.normal,
    );

    return Padding(
      padding: const EdgeInsets.symmetric(vertical: 4),
      child: Row(
        mainAxisAlignment: MainAxisAlignment.spaceBetween,
        children: [
          Text(label, style: style),
          Text(
            isFree ? 'Grátis' : '€${value.toStringAsFixed(2)}',
            style: style,
          ),
        ],
      ),
    );
  }
}
