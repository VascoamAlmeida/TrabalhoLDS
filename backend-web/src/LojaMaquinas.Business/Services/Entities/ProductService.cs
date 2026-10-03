using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Interfaces.Parameters;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Business.Validations.Entities;

namespace LojaMaquinas.Business.Services.Parameters
{
    /// <summary>
    /// Implementação do serviço de gestão de produtos.
    /// Contém lógica de negócio para validação, soft delete e monitorização de stock.
    /// </summary>
    public class ProductService : BaseService, IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IStockRepository _stockRepository;

        /// <summary>
        /// Limiar de stock baixo (5 unidades).
        /// Produtos com quantidade abaixo deste valor são considerados em stock baixo.
        /// </summary>
        private const int LOW_STOCK_THRESHOLD = 5;

        /// <summary>
        /// Inicializa uma nova instância do serviço de produtos.
        /// </summary>
        /// <param name="productRepository">Repositório para acesso aos dados de produtos</param>
        /// <param name="stockRepository">Repositório para gestão de stock</param>
        /// <param name="notifier">Serviço de notificações para comunicar erros</param>
        public ProductService(IProductRepository productRepository, IStockRepository stockRepository, INotifierService notifier) : base(notifier)
        {
            _productRepository = productRepository;
            _stockRepository = stockRepository;
        }

        /// <summary>
        /// Adiciona um novo produto ao sistema.
        /// Valida os dados, verifica duplicações e inicializa a quantidade em 0.
        /// </summary>
        /// <param name="product">Produto a ser adicionado</param>
        public async Task Add(Product product)
        {
            // 1. Run Validation of data entry and data in the database
            if (!RunValidation(new ProductValidation(), product)) return;

            if (_productRepository.Find(p => p.Id == product.Id).Result.Any())
            {
                NotifyWithMessage($"The product {product.Name} already exists!");
                return;
            }

            product.Quantity = 0;

            // 2. Run persistence
            await _productRepository.Add(product);
        }

        /// <summary>
        /// Atualiza os dados de um produto existente.
        /// Valida os dados e verifica se não existe outro produto com o mesmo nome.
        /// </summary>
        /// <param name="product">Produto com os dados atualizados</param>
        public async Task Update(Product product)
        {
            // 1. Run Validation of data entry and data in the database
            if (!RunValidation(new ProductValidation(), product)) return;

            if (!_productRepository.Find(p => p.Id == product.Id).Result.Any())
            {
                NotifyWithMessage($"The product {product.Name} does not exist!");
                return;
            }

            if (_productRepository.Find(p => p.Name == product.Name && p.Id != product.Id).Result.Any())
            {
                NotifyWithMessage($"The product {product.Name} already exists!");
                return;
            }

            // 2. Run persistence
            await _productRepository.Update(product);
        }

        /// <summary>
        /// Remove um produto do sistema (soft delete).
        /// Desativa o produto e marca todos os stocks disponíveis como indisponíveis.
        /// </summary>
        /// <param name="id">Identificador único do produto a remover</param>
        public async Task Remove(Guid id)
        {
            var product = await _productRepository.GetStocksByProductId(id);

            if (product == null)
            {
                NotifyWithMessage($"Product with ID {id} not found.");
                return;
            }

            if (product.Stocks != null && product.Stocks.Any())
            {
                var availableStocks = product.Stocks
                                             .Where(s => s.StockStatus == StockStatus.Available)
                                             .ToList();

                if (availableStocks.Any())
                {
                    foreach (var stock in availableStocks)
                    {
                        stock.StockStatus = StockStatus.NotAvailable;
                        await _stockRepository.Update(stock);
                    }
                }
            }

            product.IsActive = false;
            await _productRepository.Update(product);
        }

        /// <summary>
        /// Obtém produtos com stock abaixo do limiar definido (5 unidades).
        /// Útil para alertas de reposição de stock.
        /// </summary>
        /// <returns>Coleção de produtos com stock baixo</returns>
        public async Task<IEnumerable<Product>> GetLowStockProducts()
        {
            return await _productRepository.GetProductsWithLowStock(LOW_STOCK_THRESHOLD);
        }

        /// <summary>
        /// Liberta os recursos utilizados pelo serviço.
        /// </summary>
        public void Dispose()
        {
            _productRepository.Dispose();
        }
    }
}
