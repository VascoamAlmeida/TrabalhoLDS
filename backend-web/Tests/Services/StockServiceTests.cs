using Moq;
using System.Linq.Expressions;
using LojaMaquinas.Business.Interfaces.Parameters;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Business.Services.Parameters;
using LojaMaquinas.Business.Models;
using LojaMaquinas.Business.Interfaces.Entities;

namespace LojaMaquinas.Business.Tests.Services
{
    /// <summary>
    /// Classe de testes unitários para o serviço de gestão de stocks.
    /// Testa as operações CRUD, gestão de estados de stock e processamento de devoluções.
    /// </summary>
    public class StockServiceTests
    {
        /// <summary>
        /// Mock do repositório de stocks para simular operações na base de dados.
        /// </summary>
        private readonly Mock<IStockRepository> _stockRepositoryMock;

        /// <summary>
        /// Mock do repositório de produtos para simular operações relacionadas com produtos.
        /// </summary>
        private readonly Mock<IProductRepository> _productRepositoryMock;

        /// <summary>
        /// Mock do repositório de pedidos para simular operações relacionadas com pedidos.
        /// </summary>
        private readonly Mock<IOrderRepository> _orderRepositoryMock;

        /// <summary>
        /// Mock do repositório de alugueres para simular operações relacionadas com pedidos de aluguer.
        /// </summary>
        private readonly Mock<IOrderRentRepository> _orderRentRepositoryMock;

        /// <summary>
        /// Mock do serviço de notificações para verificar erros e validações.
        /// </summary>
        private readonly Mock<INotifierService> _notifierMock;

        /// <summary>
        /// Instância do serviço de stocks a ser testado.
        /// </summary>
        private readonly StockService _stockService;

        /// <summary>
        /// ID do produto usado nos testes.
        /// </summary>
        private readonly Guid _productId = Guid.NewGuid();

        /// <summary>
        /// ID do stock usado nos testes.
        /// </summary>
        private readonly Guid _stockId = Guid.NewGuid();

        /// <summary>
        /// ID do pedido usado nos testes.
        /// </summary>
        private readonly Guid _orderId = Guid.NewGuid();

        /// <summary>
        /// Cria um produto válido para uso nos testes.
        /// </summary>
        /// <param name="initialQuantity">Quantidade inicial do produto</param>
        /// <returns>Um produto válido com dados preenchidos</returns>
        private Product GetValidProduct(int initialQuantity = 1) => new Product
        {
            Id = _productId,
            Name = "Excavator XL3000",
            Quantity = initialQuantity,
            Image = "valid.png",
            Price = 1000m,
            YearManufacture = DateTime.Now.Year
        };

        /// <summary>
        /// Cria um stock válido para uso nos testes.
        /// </summary>
        /// <param name="status">Status do stock</param>
        /// <returns>Um stock válido com dados preenchidos</returns>
        private Stock GetValidStock(StockStatus status = StockStatus.Available) => new Stock
        {
            Id = _stockId,
            Ref = Guid.NewGuid(),
            ProductId = _productId,
            StockStatus = status,
            Orders = new List<Order>()
        };

        /// <summary>
        /// Construtor que inicializa os mocks e a instância do serviço para cada teste.
        /// </summary>
        public StockServiceTests()
        {
            _stockRepositoryMock = new Mock<IStockRepository>();
            _productRepositoryMock = new Mock<IProductRepository>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _orderRentRepositoryMock = new Mock<IOrderRentRepository>();
            _notifierMock = new Mock<INotifierService>();

            _notifierMock.Setup(x => x.HasNotification()).Returns(false);

            _stockService = new StockService(
                _stockRepositoryMock.Object,
                _productRepositoryMock.Object,
                _orderRepositoryMock.Object,
                _orderRentRepositoryMock.Object,
                _notifierMock.Object
            );
        }

        /// <summary>
        /// Testa a adição de um stock válido.
        /// Verifica se o stock é adicionado e a quantidade do produto é incrementada.
        /// </summary>
        [Fact]
        public async Task Add_ValidStock_ShouldAddStockAndUpdateProductQuantity()
        {
            // Arrange
            var stockRef = Guid.NewGuid();
            var stock = GetValidStock();
            stock.Ref = stockRef;
            var product = GetValidProduct(initialQuantity: 5);

            _stockRepositoryMock.Setup(x => x.Find(It.IsAny<Expression<Func<Stock, bool>>>()))
                .ReturnsAsync(new List<Stock>());

            _productRepositoryMock.Setup(x => x.GetById(_productId)).ReturnsAsync(product);
            _stockRepositoryMock.Setup(x => x.Add(It.IsAny<Stock>())).Returns(Task.CompletedTask);

            // Act
            await _stockService.Add(stock);

            // Assert
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Never);
            _stockRepositoryMock.Verify(x => x.Add(It.Is<Stock>(s => s.Ref == stockRef)), Times.Once);

            _productRepositoryMock.Verify(x => x.Update(It.Is<Product>(p =>
                p.Id == _productId && p.Quantity == 6)), Times.Once);
        }

        /// <summary>
        /// Testa a tentativa de adicionar um stock com ID duplicado.
        /// Verifica se o serviço notifica o erro e não adiciona o stock.
        /// </summary>
        [Fact]
        public async Task Add_DuplicateStockId_ShouldNotifyOnceAndNotAdd()
        {
            // Arrange
            var stock = GetValidStock();

            _stockRepositoryMock.Setup(x => x.Find(It.IsAny<Expression<Func<Stock, bool>>>()))
                .ReturnsAsync(new List<Stock> { stock });

            // Act
            await _stockService.Add(stock);

            // Assert
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Once);
            _stockRepositoryMock.Verify(x => x.Add(It.IsAny<Stock>()), Times.Never);
            _productRepositoryMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);
        }

        /// <summary>
        /// Testa a adição de um stock com dados inválidos (Ref vazia).
        /// Verifica se o serviço notifica os erros de validação e não adiciona o stock.
        /// </summary>
        [Fact]
        public async Task Add_InvalidStockData_ShouldNotifyValidationErrors()
        {
            // Arrange
            var invalidStock = GetValidStock();
            invalidStock.Ref = Guid.Empty;

            // Act
            await _stockService.Add(invalidStock);

            // Assert
            _stockRepositoryMock.Verify(x => x.Add(It.IsAny<Stock>()), Times.Never);
            _productRepositoryMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);

            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.AtLeastOnce);
        }

        /// <summary>
        /// Testa a atualização de um stock válido.
        /// Verifica se o stock é atualizado com sucesso.
        /// </summary>
        [Fact]
        public async Task Update_ValidStock_ShouldUpdateSuccessfully()
        {
            // Arrange
            var stockId = _stockId;
            var originalRef = Guid.NewGuid();

            var existingStock = GetValidStock();
            existingStock.Ref = originalRef;
            existingStock.StockStatus = StockStatus.NotAvailable;

            var stockToUpdate = GetValidStock();
            stockToUpdate.Id = stockId;
            stockToUpdate.Ref = originalRef;
            stockToUpdate.StockStatus = StockStatus.Available;

            existingStock.Orders = new List<Order> { new Order { Type = OrderType.Sale, Status = OrderStatus.Completed } };

            _stockRepositoryMock.SetupSequence(x => x.GetStockWithOrdersAndProduct(stockId))
                .ReturnsAsync(existingStock);

            _stockRepositoryMock.Setup(x => x.Find(It.IsAny<Expression<Func<Stock, bool>>>()))
                .ReturnsAsync(new List<Stock>());

            // Act
            await _stockService.Update(stockToUpdate);

            // Assert
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Never);

            _stockRepositoryMock.Verify(x => x.Update(It.Is<Stock>(s =>
                s.Ref == originalRef && s.StockStatus == StockStatus.Available
            )), Times.Once);
        }

        /// <summary>
        /// Testa a tentativa de atualização de um stock inexistente.
        /// Verifica se o serviço notifica o erro e não atualiza.
        /// </summary>
        [Fact]
        public async Task Update_StockNotFound_ShouldNotifyOnceAndNotUpdate()
        {
            // Arrange
            var stockToUpdate = GetValidStock();
            _stockRepositoryMock.Setup(x => x.GetStockWithOrdersAndProduct(_stockId))
                .ReturnsAsync((Stock)null);

            // Act
            await _stockService.Update(stockToUpdate);

            // Assert
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Once);
            _stockRepositoryMock.Verify(x => x.Update(It.IsAny<Stock>()), Times.Never);
        }

        /// <summary>
        /// Testa a tentativa de atualização com referência duplicada.
        /// Verifica se o serviço notifica o erro e não atualiza.
        /// </summary>
        [Fact]
        public async Task Update_DuplicateRef_ShouldNotifyOnceAndNotUpdate()
        {
            // Arrange
            var stockToUpdate = GetValidStock();
            var existingStockWithSameRef = GetValidStock();
            existingStockWithSameRef.Id = Guid.NewGuid();

            _stockRepositoryMock.Setup(x => x.GetStockWithOrdersAndProduct(_stockId))
                .ReturnsAsync(stockToUpdate);

            _stockRepositoryMock.Setup(x => x.Find(It.IsAny<Expression<Func<Stock, bool>>>()))
                .ReturnsAsync(new List<Stock> { existingStockWithSameRef });

            // Act
            await _stockService.Update(stockToUpdate);

            // Assert
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Once);
            _stockRepositoryMock.Verify(x => x.Update(It.IsAny<Stock>()), Times.Never);
        }

        /// <summary>
        /// Testa a remoção de um stock disponível.
        /// Verifica se o stock é removido e a quantidade do produto é reduzida.
        /// </summary>
        [Fact]
        public async Task Remove_AvailableStock_ShouldRemoveStockAndReduceProductQuantity()
        {
            // Arrange
            var product = GetValidProduct(initialQuantity: 10);
            var stock = GetValidStock(StockStatus.Available);
            stock.Product = product;

            _stockRepositoryMock.Setup(x => x.GetStockWithOrdersAndProduct(_stockId)).ReturnsAsync(stock);
            _stockRepositoryMock.Setup(x => x.RemoveSpecific(It.IsAny<Stock>())).Returns(Task.CompletedTask);

            // Act
            await _stockService.Remove(_stockId);

            // Assert
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Never);
            _stockRepositoryMock.Verify(x => x.RemoveSpecific(stock), Times.Once);

            _productRepositoryMock.Verify(x => x.Update(It.Is<Product>(p => p.Quantity == 9)), Times.Once);
        }

        /// <summary>
        /// Testa a tentativa de remoção de um stock não disponível (SoldOut).
        /// Verifica se o serviço não remove o stock e não notifica erro.
        /// </summary>
        [Fact]
        public async Task Remove_NotAvailableStock_ShouldNotRemoveAndNotNotify()
        {
            // Arrange
            var stock = GetValidStock(StockStatus.SoldOut);
            stock.Product = GetValidProduct(initialQuantity: 10);

            _stockRepositoryMock.Setup(x => x.GetStockWithOrdersAndProduct(_stockId)).ReturnsAsync(stock);

            // Act
            await _stockService.Remove(_stockId);

            // Assert
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Never);
            _stockRepositoryMock.Verify(x => x.RemoveSpecific(It.IsAny<Stock>()), Times.Never);
            _productRepositoryMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);
        }

        /// <summary>
        /// Testa a tentativa de remoção de um stock inexistente.
        /// Verifica se o serviço notifica o erro e não remove.
        /// </summary>
        [Fact]
        public async Task Remove_StockNotFound_ShouldNotifyOnceAndNotRemove()
        {
            // Arrange
            _stockRepositoryMock.Setup(x => x.GetStockWithOrdersAndProduct(_stockId)).ReturnsAsync((Stock)null);

            // Act
            await _stockService.Remove(_stockId);

            // Assert
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Once);
            _stockRepositoryMock.Verify(x => x.RemoveSpecific(It.IsAny<Stock>()), Times.Never);
        }

        /// <summary>
        /// Testa o processamento de devolução de um aluguer ativo.
        /// Verifica se o stock fica disponível, a quantidade do produto aumenta e os status são atualizados.
        /// </summary>
        [Fact]
        public async Task ProcessRentReturn_ActiveRentFound_ShouldUpdateStatusesAndIncreaseProductQuantity()
        {
            // Arrange
            var product = GetValidProduct(initialQuantity: 5);
            var orderRent = new OrderRent { Status = OrderRentStatus.Active };
            var activeRentOrder = new Order
            {
                Id = _orderId,
                OrderDate = DateTime.Now,
                Type = OrderType.Rent,
                Status = OrderStatus.Pending,
                OrderRent = orderRent
            };
            var stock = GetValidStock(StockStatus.NotAvailable);
            stock.Product = product;
            stock.Orders = new List<Order> { activeRentOrder };

            _stockRepositoryMock.Setup(x => x.GetStockWithOrdersAndProduct(_stockId)).ReturnsAsync(stock);

            // Act
            await _stockService.ProcessRentReturn(_stockId);

            // Assert
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Never);

            // Verify Stock Status is set to Available
            _stockRepositoryMock.Verify(x => x.Update(It.Is<Stock>(s => s.StockStatus == StockStatus.Available)), Times.Once);

            // Verify Product Quantity increased from 5 to 6
            _productRepositoryMock.Verify(x => x.Update(It.Is<Product>(p => p.Quantity == 6)), Times.Once);

            // Verify Order Status is set to Completed
            _orderRepositoryMock.Verify(x => x.Update(It.Is<Order>(o => o.Status == OrderStatus.Completed)), Times.Once);

            // Verify OrderRent Status is set to Completed
            _orderRentRepositoryMock.Verify(x => x.Update(It.Is<OrderRent>(or => or.Status == OrderRentStatus.Completed)), Times.Once);
        }

        /// <summary>
        /// Testa o processamento de devolução com stock inexistente.
        /// Verifica se o serviço notifica o erro e não atualiza nada.
        /// </summary>
        [Fact]
        public async Task ProcessRentReturn_InvalidState_StockNotFound_ShouldNotifyOnceAndNotUpdate()
        {
            // Arrange
            Stock stock = null;

            _stockRepositoryMock.Setup(x => x.GetStockWithOrdersAndProduct(_stockId)).ReturnsAsync(stock);

            // Act
            await _stockService.ProcessRentReturn(_stockId);

            // Assert
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Exactly(1));
            _stockRepositoryMock.Verify(x => x.Update(It.IsAny<Stock>()), Times.Never);
            _productRepositoryMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);
            _orderRepositoryMock.Verify(x => x.Update(It.IsAny<Order>()), Times.Never);
        }

        /// <summary>
        /// Testa o processamento de devolução com stock já disponível.
        /// Verifica se o serviço notifica o erro e não atualiza.
        /// </summary>
        [Fact]
        public async Task ProcessRentReturn_InvalidState_AlreadyAvailable_ShouldNotifyOnceAndNotUpdate()
        {
            // Arrange
            Stock stock = GetValidStock(StockStatus.Available);

            _stockRepositoryMock.Setup(x => x.GetStockWithOrdersAndProduct(_stockId)).ReturnsAsync(stock);

            // Act
            await _stockService.ProcessRentReturn(_stockId);

            // Assert
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Exactly(1));
            _stockRepositoryMock.Verify(x => x.Update(It.IsAny<Stock>()), Times.Never);
            _productRepositoryMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);
            _orderRepositoryMock.Verify(x => x.Update(It.IsAny<Order>()), Times.Never);
        }

        /// <summary>
        /// Testa o processamento de cancelamento/devolução de uma venda completa.
        /// Verifica se o stock fica disponível, a quantidade do produto aumenta e o pedido é cancelado.
        /// </summary>
        [Fact]
        public async Task ProcessCancelReturn_CompletedSaleFound_ShouldUpdateStatusesAndIncreaseProductQuantity()
        {
            // Arrange
            var product = GetValidProduct(initialQuantity: 8);
            var completedSaleOrder = new Order
            {
                Id = _orderId,
                OrderDate = DateTime.Now,
                Type = OrderType.Sale,
                Status = OrderStatus.Completed
            };
            var stock = GetValidStock(StockStatus.SoldOut);
            stock.Product = product;
            stock.Orders = new List<Order> { completedSaleOrder };

            _stockRepositoryMock.Setup(x => x.GetStockWithOrdersAndProduct(_stockId)).ReturnsAsync(stock);

            // Act
            await _stockService.ProcessCancelReturn(_stockId);

            // Assert
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Never);

            // Verify Stock Status is set to Available
            _stockRepositoryMock.Verify(x => x.Update(It.Is<Stock>(s => s.StockStatus == StockStatus.Available)), Times.Once);

            // Verify Product Quantity increased from 8 to 9
            _productRepositoryMock.Verify(x => x.Update(It.Is<Product>(p => p.Quantity == 9)), Times.Once);

            // Verify Order Status is set to Cancelled
            _orderRepositoryMock.Verify(x => x.Update(It.Is<Order>(o => o.Status == OrderStatus.Cancelled)), Times.Once);
        }

        /// <summary>
        /// Testa o processamento de cancelamento quando não há venda completa associada.
        /// Verifica se o serviço notifica o erro e não atualiza.
        /// </summary>
        [Fact]
        public async Task ProcessCancelReturn_NoCompletedSaleFound_ShouldNotifyOnceAndNotUpdate()
        {
            // Arrange
            var product = GetValidProduct(initialQuantity: 8);
            var nonTargetOrder = new Order
            {
                Id = _orderId,
                OrderDate = DateTime.Now,
                Type = OrderType.Rent,
                Status = OrderStatus.Completed
            };
            var stock = GetValidStock(StockStatus.SoldOut);
            stock.Product = product;
            stock.Orders = new List<Order> { nonTargetOrder };

            _stockRepositoryMock.Setup(x => x.GetStockWithOrdersAndProduct(_stockId)).ReturnsAsync(stock);

            // Act
            await _stockService.ProcessCancelReturn(_stockId);

            // Assert
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Once);
            _stockRepositoryMock.Verify(x => x.Update(It.IsAny<Stock>()), Times.Never);
            _orderRepositoryMock.Verify(x => x.Update(It.IsAny<Order>()), Times.Never);
        }
    }
}