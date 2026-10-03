import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../models/client.dart';
import '../models/order.dart';
import '../providers/auth_provider.dart';
import '../providers/client_provider.dart';
import '../providers/orders_provider.dart';

class OrderListPage extends StatefulWidget {
  const OrderListPage({super.key});

  @override
  State<OrderListPage> createState() => _OrderListPageState();
}

class _OrderListPageState extends State<OrderListPage> {
  Client? _client;
  bool _isLoadingClient = true;
  bool _loadRequested = false;
  String _filterStatus = 'Todos';

  @override
  void didChangeDependencies() {
    super.didChangeDependencies();
    if (!_loadRequested) {
      _loadRequested = true;
      WidgetsBinding.instance.addPostFrameCallback((_) {
        if (mounted) _loadClientAndOrders();
      });
    }
  }

  Future<void> _loadClientAndOrders() async {
    final authProvider = Provider.of<AuthProvider>(context, listen: false);
    final userId = authProvider.user?.id;

    if (userId == null || userId.isEmpty) {
      if (mounted) setState(() => _isLoadingClient = false);
      return;
    }

    final clientProvider = Provider.of<ClientProvider>(context, listen: false);
    final clientResult = await clientProvider.fetchClientByUserId(userId);

    if (!mounted) return;
    setState(() {
      _client = clientResult;
      _isLoadingClient = false;
    });

    if (clientResult != null && clientResult.id.isNotEmpty) {
      await Provider.of<OrdersProvider>(context, listen: false)
          .fetchOrdersByClient(clientResult.id);
    }
  }

  List<Order> _applyFilters(List<Order> orders) {
    if (_filterStatus == 'Todos') return orders;
    return orders.where((order) => order.status.name == _filterStatus).toList();
  }

  Color _statusColor(OrderStatus status) {
    switch (status) {
      case OrderStatus.pending:
        return Colors.orange;
      case OrderStatus.active:
        return Colors.blue;
      case OrderStatus.completed:
        return Colors.green;
      case OrderStatus.cancelled:
        return Colors.red;
    }
  }

  String _statusLabel(OrderStatus status) {
    switch (status) {
      case OrderStatus.pending:
        return 'Pendente';
      case OrderStatus.active:
        return 'Ativa';
      case OrderStatus.completed:
        return 'Concluída';
      case OrderStatus.cancelled:
        return 'Cancelada';
    }
  }

  String _typeLabel(OrderType type) =>
      type == OrderType.sale ? 'Venda' : 'Aluguer';

  String _formatDate(DateTime date) {
    return '${date.day.toString().padLeft(2, '0')}/'
        '${date.month.toString().padLeft(2, '0')}/'
        '${date.year}';
  }

  @override
  Widget build(BuildContext context) {
    final provider = Provider.of<OrdersProvider>(context);
    final clientProvider = Provider.of<ClientProvider>(context);

    return Scaffold(
      appBar: AppBar(
        title: const Text('As minhas encomendas'),
        actions: [
          PopupMenuButton<String>(
            onSelected: (value) => setState(() => _filterStatus = value),
            itemBuilder: (context) => const [
              PopupMenuItem(value: 'Todos', child: Text('Todos')),
              PopupMenuItem(value: 'pending', child: Text('Pendentes')),
              PopupMenuItem(value: 'active', child: Text('Ativas')),
              PopupMenuItem(value: 'completed', child: Text('Concluídas')),
              PopupMenuItem(value: 'cancelled', child: Text('Canceladas')),
            ],
          ),
        ],
      ),
      body: _isLoadingClient
          ? const Center(child: CircularProgressIndicator())
          : _client == null
              ? _buildMissingClient(clientProvider.errorMessage)
              : provider.isLoading
                  ? const Center(child: CircularProgressIndicator())
                  : provider.errorMessage != null && provider.orders.isEmpty
                      ? _buildError(provider.errorMessage!)
                      : RefreshIndicator(
                          onRefresh: () =>
                              provider.fetchOrdersByClient(_client!.id),
                          child: _buildOrderList(_applyFilters(provider.orders)),
                        ),
    );
  }

  Widget _buildMissingClient(String? errorMessage) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.person_off_outlined, size: 48),
            const SizedBox(height: 12),
            const Text(
              'Perfil de cliente em falta',
              style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold),
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 8),
            const Text(
              'Complete o perfil de cliente na aplicação Web para consultar e criar encomendas.',
              textAlign: TextAlign.center,
            ),
            if (errorMessage != null) ...[
              const SizedBox(height: 8),
              Text(
                errorMessage,
                style: const TextStyle(color: Colors.grey),
                textAlign: TextAlign.center,
              ),
            ],
          ],
        ),
      ),
    );
  }

  Widget _buildError(String message) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(message, textAlign: TextAlign.center),
            const SizedBox(height: 12),
            ElevatedButton(
              onPressed: _client == null
                  ? null
                  : () => Provider.of<OrdersProvider>(context, listen: false)
                      .fetchOrdersByClient(_client!.id),
              child: const Text('Tentar novamente'),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildOrderList(List<Order> orders) {
    if (orders.isEmpty) {
      return ListView(
        physics: const AlwaysScrollableScrollPhysics(),
        children: const [
          SizedBox(height: 160),
          Center(child: Text('Sem encomendas')),
        ],
      );
    }

    return ListView.builder(
      physics: const AlwaysScrollableScrollPhysics(),
      padding: const EdgeInsets.all(10),
      itemCount: orders.length,
      itemBuilder: (context, index) {
        final order = orders[index];
        final shortStockId = order.stockId.length > 8
            ? order.stockId.substring(0, 8)
            : order.stockId;

        return Card(
          margin: const EdgeInsets.symmetric(vertical: 6),
          child: Padding(
            padding: const EdgeInsets.all(12),
            child: Row(
              children: [
                const SizedBox(
                  width: 50,
                  height: 50,
                  child: Icon(Icons.shopping_bag),
                ),
                const SizedBox(width: 12),
                Expanded(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.start,
                    children: [
                      Text(
                        '${_typeLabel(order.type)} · Stock $shortStockId',
                        style: const TextStyle(
                          fontSize: 16,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                      Text('${order.total.toStringAsFixed(2)} €'),
                      Text(_formatDate(order.orderDate)),
                    ],
                  ),
                ),
                Container(
                  padding: const EdgeInsets.symmetric(
                    horizontal: 12,
                    vertical: 6,
                  ),
                  decoration: BoxDecoration(
                    borderRadius: BorderRadius.circular(20),
                    color: _statusColor(order.status).withOpacity(0.15),
                  ),
                  child: Text(
                    _statusLabel(order.status),
                    style: TextStyle(
                      color: _statusColor(order.status),
                      fontSize: 12,
                      fontWeight: FontWeight.bold,
                    ),
                  ),
                ),
              ],
            ),
          ),
        );
      },
    );
  }
}
