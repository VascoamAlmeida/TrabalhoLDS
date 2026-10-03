import 'package:flutter/material.dart';
import 'package:provider/provider.dart';

import '../../models/product.dart';
import '../../models/stock.dart';
import '../../providers/products_provider.dart';

class CatalogScreen extends StatefulWidget {
  const CatalogScreen({super.key});

  @override
  State<CatalogScreen> createState() => _CatalogScreenState();
}

class _CatalogScreenState extends State<CatalogScreen> {
  @override
  void initState() {
    super.initState();
    Future.microtask(() {
      if (mounted) {
        Provider.of<ProductsProvider>(context, listen: false).fetchProducts();
      }
    });
  }

  @override
  Widget build(BuildContext context) {
    final provider = Provider.of<ProductsProvider>(context);
    final products = provider.products;

    return Scaffold(
      appBar: AppBar(title: const Text('Catálogo')),
      body: provider.isLoading
          ? const Center(child: CircularProgressIndicator())
          : provider.errorMessage != null && products.isEmpty
              ? _buildError(provider)
              : RefreshIndicator(
                  onRefresh: () => provider.fetchProducts(),
                  child: products.isEmpty
                      ? ListView(
                          physics: const AlwaysScrollableScrollPhysics(),
                          children: const [
                            SizedBox(height: 160),
                            Center(child: Text('Sem produtos disponíveis')),
                          ],
                        )
                      : ListView.builder(
                          physics: const AlwaysScrollableScrollPhysics(),
                          padding: const EdgeInsets.all(12),
                          itemCount: products.length,
                          itemBuilder: (context, index) {
                            return _buildProductCard(products[index]);
                          },
                        ),
                ),
    );
  }

  Widget _buildError(ProductsProvider provider) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(
              provider.errorMessage ?? 'Erro ao carregar produtos',
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 12),
            ElevatedButton(
              onPressed: provider.fetchProducts,
              child: const Text('Tentar novamente'),
            ),
          ],
        ),
      ),
    );
  }

  Widget _buildProductCard(Product product) {
    final typeText = product.type == ProductType.Machine ? 'Máquina' : 'Peça';
    final availableStocks = product.stocks
        .where((stock) => stock.stockStatus == StockStatus.Available)
        .toList();
    final hasAvailableStock = product.isActive && availableStocks.isNotEmpty;

    return Card(
      elevation: 4,
      margin: const EdgeInsets.only(bottom: 14),
      child: Padding(
        padding: const EdgeInsets.all(14),
        child: Column(
          crossAxisAlignment: CrossAxisAlignment.start,
          children: [
            Text(
              product.name,
              style: const TextStyle(
                fontSize: 18,
                fontWeight: FontWeight.w700,
              ),
            ),
            const SizedBox(height: 6),
            Text('Marca: ${product.brand}'),
            Text('Tipo: $typeText'),
            Text(
              'Disponíveis: ${availableStocks.length}',
              style: TextStyle(
                color: hasAvailableStock ? Colors.green : Colors.red,
                fontWeight: FontWeight.bold,
              ),
            ),
            const SizedBox(height: 12),
            Wrap(
              spacing: 10,
              runSpacing: 8,
              children: [
                ElevatedButton.icon(
                  onPressed: () {
                    Navigator.pushNamed(
                      context,
                      '/product-details',
                      arguments: product,
                    );
                  },
                  icon: const Icon(Icons.visibility),
                  label: const Text('Detalhes'),
                ),
                if (hasAvailableStock)
                  ElevatedButton.icon(
                    onPressed: () {
                      Navigator.pushNamed(
                        context,
                        '/create-order',
                        arguments: {
                          'stock': availableStocks.first,
                          'product': product,
                        },
                      );
                    },
                    icon: const Icon(Icons.shopping_cart),
                    label: const Text('Fazer Pedido'),
                    style: ElevatedButton.styleFrom(
                      backgroundColor: Colors.green,
                    ),
                  ),
              ],
            ),
          ],
        ),
      ),
    );
  }
}
