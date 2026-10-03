using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Interfaces.Parameters;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Business.Validations.Entities;

namespace LojaMaquinas.Business.Services.Parameters
{
    /// <summary>
    /// Implementação do serviço de gestão de inventário (Stock).
    /// Contém lógica de negócio para controlo de stock, devoluções e processamento de alugueres.
    /// </summary>
    public class StockService : BaseService, IStockService
    {
        private readonly IStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderRentRepository _orderRentRepository;

        /// <summary>
        /// Inicializa uma nova instância do serviço de stock.
        /// </summary>
        /// <param name="stockRepository">Repositório para acesso aos dados de stock</param>
        /// <param name="productRepository">Repositório para gestão de produtos</param>
        /// <param name="orderRepository">Repositório para gestão de pedidos</param>
        /// <param name="orderRentRepository">Repositório para gestão de alugueres</param>
        /// <param name="notifier">Serviço de notificações para comunicar erros</param>
        public StockService(IStockRepository stockRepository, IProductRepository productRepository, IOrderRepository orderRepository,
            IOrderRentRepository orderRentRepository, INotifierService notifier) : base(notifier)
        {
            _stockRepository = stockRepository;
            _productRepository = productRepository;
            _orderRepository = orderRepository;
            _orderRentRepository = orderRentRepository;
        }

        /// <summary>
        /// Adiciona um novo item de stock ao sistema.
        /// Valida os dados, verifica duplicações e incrementa a quantidade do produto associado.
        /// </summary>
        /// <param name="stock">Item de stock a ser adicionado</param>
        public async Task Add(Stock stock)
        {
            // 1. Run Validation of data entry and data in the database
            if (!RunValidation(new StockValidation(), stock)) return;

            if (_stockRepository.Find(p => p.Id == stock.Id).Result.Any() || _stockRepository.Find(p => p.Ref == stock.Ref).Result.Any())
            {
                NotifyWithMessage($"The stock item {stock.Ref} already exists!");
                return;
            }

            // 2. Run persistence
            await _stockRepository.Add(stock);

            var product = await _productRepository.GetById(stock.ProductId);
            product.Quantity += 1;
            await _productRepository.Update(product);
        }

        /// <summary>
        /// Atualiza os dados de um item de stock existente.
        /// Valida os dados e verifica se a referência não está a ser usada por outro item.
        /// </summary>
        /// <param name="stock">Stock com os dados atualizados</param>
        public async Task Update(Stock stock)
        {
            var existingStock = await _stockRepository.GetStockWithOrdersAndProduct(stock.Id);

            if (existingStock == null)
            {
                NotifyWithMessage($"Stock item with ID {stock.Id} not found.");
                return;
            }

            existingStock.Ref = stock.Ref;
            existingStock.ProductId = stock.ProductId;
            existingStock.StockStatus = stock.StockStatus;

            if (!RunValidation(new StockValidation(), existingStock)) return;

            if (_stockRepository.Find(s => s.Ref == existingStock.Ref && s.Id != existingStock.Id).Result.Any())
            {
                NotifyWithMessage($"The stock item {existingStock.Ref} is already used by another item!");
                return;
            }

            // Run persistence
            await _stockRepository.Update(existingStock);
        }

        /// <summary>
        /// Remove um item de stock do sistema.
        /// Apenas permite remoção se o stock estiver disponível e decrementa a quantidade do produto.
        /// </summary>
        /// <param name="id">Identificador único do stock a remover</param>
        public async Task Remove(Guid id)
        {
            // Fetch the stock item with its related orders and product details
            var stock = await _stockRepository.GetStockWithOrdersAndProduct(id);

            if (stock == null)
            {
                NotifyWithMessage($"Stock item with ID {id} not found.");
                return;
            }

            // Check if the stock item is not sold out, already not available or is rented out
            bool isAvailable = (stock.StockStatus == StockStatus.Available);

            if (isAvailable)
            {
                // Reduce Product Quantity.
                if (stock.Product != null && stock.Product.Quantity > 0)
                {
                    stock.Product.Quantity -= 1;
                    await _productRepository.Update(stock.Product);
                }
            }
            else
            {
                return;
            }

            await _stockRepository.RemoveSpecific(stock);
        }

        /// <summary>
        /// Processa a devolução de um equipamento alugado.
        /// Altera o estado do stock para Available, atualiza o pedido e aluguer para Completed,
        /// e incrementa a quantidade do produto.
        /// </summary>
        /// <param name="stockId">Identificador único do stock devolvido</param>
        public async Task ProcessRentReturn(Guid stockId)
        {
            var stock = await _stockRepository.GetStockWithOrdersAndProduct(stockId);

            if (stock == null || stock.StockStatus == StockStatus.Available)
            {
                NotifyWithMessage($"Stock item with id {stockId} has already been returned or does not exist.");
                return;
            }

            var currentRentOrder = stock.Orders
                .OrderByDescending(o => o.OrderDate)
                .FirstOrDefault(o =>
                    o.Type == OrderType.Rent &&
                    o.OrderRent != null &&
                    o.Status != OrderStatus.Completed &&
                    o.OrderRent.Status != OrderRentStatus.Completed);

            if (currentRentOrder == null)
            {
                NotifyWithMessage($"Stock item {stock.Ref} is not currently linked to an active Rent order or has already been returned.");
                return;
            }

            stock.StockStatus = StockStatus.Available;
            await _stockRepository.Update(stock);

            stock.Product.Quantity += 1;
            await _productRepository.Update(stock.Product);

            currentRentOrder.Status = OrderStatus.Completed;
            await _orderRepository.Update(currentRentOrder);

            currentRentOrder.OrderRent.Status = OrderRentStatus.Completed;
            await _orderRentRepository.Update(currentRentOrder.OrderRent);
        }

        /// <summary>
        /// Processa o cancelamento de uma venda e devolução do stock.
        /// Altera o estado do stock para Available, marca o pedido como Cancelled,
        /// e incrementa a quantidade do produto.
        /// </summary>
        /// <param name="stockId">Identificador único do stock a devolver</param>
        public async Task ProcessCancelReturn(Guid stockId)
        {
            var stock = await _stockRepository.GetStockWithOrdersAndProduct(stockId);

            if (stock == null || stock.StockStatus == StockStatus.Available)
            {
                NotifyWithMessage($"Stock item with id {stockId} has already been returned or does not exist.");
                return;
            }

            var cancelledOrder = stock.Orders
                .OrderByDescending(o => o.OrderDate)
                .FirstOrDefault(o =>
                    o.Type == OrderType.Sale &&
                    o.Status == OrderStatus.Completed);

            if (cancelledOrder == null)
            {
                NotifyWithMessage($"Stock item {stock.Ref} is not currently linked to an active order or has already been returned.");
                return;
            }

            stock.StockStatus = StockStatus.Available;
            await _stockRepository.Update(stock);

            stock.Product.Quantity += 1;
            await _productRepository.Update(stock.Product);

            cancelledOrder.Status = OrderStatus.Cancelled;
            await _orderRepository.Update(cancelledOrder);
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
