using FluentAssertions;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Business.Services.Parameters;
using Moq;
using System.Linq.Expressions;
using LojaMaquinas.Business.Models;
using LojaMaquinas.Business.Interfaces.Parameters;

namespace LojaMaquinas.Business.Tests.Services
{
    /// <summary>
    /// Classe de testes unitários para o serviço de gestão de produtos.
    /// Testa as operações CRUD, validações e gestão de stock dos produtos.
    /// </summary>
    public class ProductServiceTests
    {
        /// <summary>
        /// Mock do repositório de produtos para simular operações na base de dados.
        /// </summary>
        private readonly Mock<IProductRepository> _productRepositoryMock;

        /// <summary>
        /// Mock do repositório de stocks para simular operações relacionadas com stocks.
        /// </summary>
        private readonly Mock<IStockRepository> _stockRepositoryMock;

        /// <summary>
        /// Mock do serviço de notificações para verificar erros e validações.
        /// </summary>
        private readonly Mock<INotifierService> _notifierMock;

        /// <summary>
        /// Instância do serviço de produtos a ser testado.
        /// </summary>
        private readonly ProductService _productService;

        /// <summary>
        /// Construtor que inicializa os mocks e a instância do serviço para cada teste.
        /// </summary>
        public ProductServiceTests()
        {
            _productRepositoryMock = new Mock<IProductRepository>();
            _stockRepositoryMock = new Mock<IStockRepository>();
            _notifierMock = new Mock<INotifierService>();
            _productService = new ProductService(
                _productRepositoryMock.Object,
                _stockRepositoryMock.Object,
                _notifierMock.Object
            );
        }

        /// <summary>
        /// Testa a adição de um produto válido.
        /// Verifica se o produto é adicionado com quantidade definida a zero automaticamente.
        /// </summary>
        [Fact]
        public async Task Add_ValidProduct_ShouldAddSuccessfullyWithQuantityZero()
        {
            // Arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Excavator XL3000",
                Type = ProductType.Machine,
                Brand = "CAT",
                Model = "XL3000",
                YearManufacture = 2023,
                Description = "Heavy duty excavator",
                Image = "excavator_image.png",
                Price = 150000.00m,
                Quantity = 10,
                CategoryId = Guid.NewGuid(),
                IsActive = true
            };

            _productRepositoryMock.Setup(x => x.Find(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product>());

            _productRepositoryMock.Setup(x => x.Add(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            // Act
            await _productService.Add(product);

            // Assert
            _productRepositoryMock.Verify(x => x.Add(It.Is<Product>(p =>
                p.Name == "Excavator XL3000" &&
                p.Quantity == 0 &&
                p.Price == 150000.00m
            )), Times.Once);

            // Verify no error notification was sent
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Never);
        }

        /// <summary>
        /// Testa a adição de um produto com preço inválido (negativo).
        /// Verifica se o serviço notifica os erros de validação e não adiciona o produto.
        /// </summary>
        [Fact]
        public async Task Add_InvalidProduct_ShouldNotifyValidationErrors()
        {
            // Arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Invalid Product",
                Type = ProductType.Machine,
                Brand = "CAT",
                Model = "XL3000",
                YearManufacture = 2023,
                Description = "Invalid product",
                Price = -1000.00m,
                CategoryId = Guid.NewGuid(),
                IsActive = true
            };

            // Act
            await _productService.Add(product);

            // Assert
            _productRepositoryMock.Verify(x => x.Add(It.IsAny<Product>()), Times.Never);
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.AtLeastOnce);
        }

        /// <summary>
        /// Testa a tentativa de adicionar um produto duplicado (mesmo nome, marca e modelo).
        /// Verifica se o serviço notifica o erro e não adiciona o produto.
        /// </summary>
        [Fact]
        public async Task Add_DuplicateProduct_ShouldNotify()
        {
            // Arrange
            var existingProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Excavator XL3000",
                Type = ProductType.Machine,
                Brand = "CAT",
                Model = "XL3000",
                YearManufacture = 2023,
                Price = 150000.00m,
                CategoryId = Guid.NewGuid()
            };

            var newProduct = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Excavator XL3000",
                Type = ProductType.Machine,
                Brand = "CAT",
                Model = "XL3000",
                YearManufacture = 2023,
                Price = 150000.00m,
                CategoryId = Guid.NewGuid()
            };

            _productRepositoryMock.Setup(x => x.Find(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product> { existingProduct });

            // Act
            await _productService.Add(newProduct);

            // Assert
            _productRepositoryMock.Verify(x => x.Add(It.IsAny<Product>()), Times.Never);

            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Exactly(2));
        }

        /// <summary>
        /// Testa a atualização de um produto válido.
        /// Verifica se o produto é atualizado com sucesso e não gera notificações de erro.
        /// </summary>
        [Fact]
        public async Task Update_ValidProduct_ShouldUpdateSuccessfully()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var productId = Guid.NewGuid();

            var product = new Product
            {
                Id = productId,
                Name = "Updated Excavator",
                Type = ProductType.Part,
                Brand = "CAT",
                Model = "XL3000",
                YearManufacture = 2022,
                Description = "Updated machine description",
                Image = "updated_image.png",
                Price = 140000.00m,
                Quantity = 10,
                CategoryId = Guid.NewGuid(),
                IsActive = true
            };

            _productRepositoryMock.SetupSequence(x => x.Find(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product> { product })
                .ReturnsAsync(new List<Product>());

            _productRepositoryMock.Setup(x => x.Update(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            _notifierMock.Setup(x => x.HasNotification()).Returns(false);

            // Act
            await _productService.Update(product);

            // Assert
            _productRepositoryMock.Verify(x => x.Update(It.Is<Product>(p =>
                p.Id == productId &&
                p.Name == "Updated Excavator" &&
                p.Price == 140000.00m
            )), Times.Once);

            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Never);
        }

        /// <summary>
        /// Testa a tentativa de atualização de um produto inexistente.
        /// Verifica se o serviço notifica o erro quando o produto não é encontrado.
        /// </summary>
        [Fact]
        public async Task Update_ProductNotFound_ShouldNotify()
        {
            // Arrange
            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Non-existent Product",
                Type = ProductType.Part,
                Brand = "CAT",
                Model = "XL3000",
                YearManufacture = 2023,
                Description = "Description is now long enough",
                Image = "valid_image.png",
                Price = 100.00m,
                Quantity = 1,
                CategoryId = Guid.NewGuid(),
                IsActive = true
            };
            
            _productRepositoryMock.Setup(x => x.Find(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product>());

            // Act
            await _productService.Update(product);

            // Assert
            _productRepositoryMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);

            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Once);
        }

        /// <summary>
        /// Testa a tentativa de atualização de um produto para um nome já existente.
        /// Verifica se o serviço notifica o erro de nome duplicado.
        /// </summary>
        [Fact]
        public async Task Update_DuplicateName_ShouldNotify()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var existingProductId = Guid.NewGuid();
            var duplicateName = "Excavator XL3000";

            var productToUpdate = new Product
            {
                Id = productId,
                Name = duplicateName,
                Type = ProductType.Part,
                Brand = "CAT",
                Model = "XL3000",
                YearManufacture = 2023,
                Description = "Valid description for update",
                Image = "valid_image_update.png",
                Price = 100.00m,
                Quantity = 1,
                CategoryId = Guid.NewGuid(),
                IsActive = true
            };

            var existingProductWithSameName = new Product
            {
                Id = existingProductId,
                Name = duplicateName,
            };

            _productRepositoryMock.SetupSequence(x => x.Find(It.IsAny<Expression<Func<Product, bool>>>()))
                .ReturnsAsync(new List<Product> { productToUpdate })
                .ReturnsAsync(new List<Product> { existingProductWithSameName });

            // Act
            await _productService.Update(productToUpdate);

            // Assert
            _productRepositoryMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);

            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Once);
        }

        /// <summary>
        /// Testa a tentativa de remoção de um produto inexistente.
        /// Verifica se o serviço notifica o erro quando o produto não é encontrado.
        /// </summary>
        [Fact]
        public async Task Remove_ProductNotFound_ShouldNotify()
        {
            // Arrange
            var productId = Guid.NewGuid();

            _productRepositoryMock.Setup(x => x.GetStocksByProductId(productId))
                 .ReturnsAsync((Product)null);

            // Act
            await _productService.Remove(productId);

            // Assert
            _productRepositoryMock.Verify(x => x.Update(It.IsAny<Product>()), Times.Never);

            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Once);
        }

        /// <summary>
        /// Testa a remoção de um produto com stocks disponíveis.
        /// Verifica se os stocks são marcados como não disponíveis e o produto é desativado.
        /// </summary>
        [Fact]
        public async Task Remove_ProductWithAvailableStocks_ShouldMarkStocksNotAvailableAndDeactivateProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var stock1 = new Stock
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                StockStatus = StockStatus.Available
            };
            var stock2 = new Stock
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                StockStatus = StockStatus.Available
            };

            var product = new Product
            {
                Id = productId,
                Name = "Excavator XL3000",
                Type = ProductType.Machine,
                Brand = "CAT",
                Model = "XL3000",
                YearManufacture = 2023,
                Price = 150000.00m,
                CategoryId = Guid.NewGuid(),
                IsActive = true,
                Stocks = new List<Stock> { stock1, stock2 }
            };

            _productRepositoryMock.Setup(x => x.GetStocksByProductId(productId))
                .ReturnsAsync(product);

            _stockRepositoryMock.Setup(x => x.Update(It.IsAny<Stock>()))
                .Returns(Task.CompletedTask);

            _productRepositoryMock.Setup(x => x.Update(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            // Act
            await _productService.Remove(productId);

            // Assert
            _stockRepositoryMock.Verify(x => x.Update(It.Is<Stock>(s =>
                s.StockStatus == StockStatus.NotAvailable
            )), Times.Exactly(2));

            _productRepositoryMock.Verify(x => x.Update(It.Is<Product>(p =>
                p.Id == productId &&
                p.IsActive == false
            )), Times.Once);

            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Never);
        }

        /// <summary>
        /// Testa a remoção de um produto sem stocks associados.
        /// Verifica se apenas o produto é desativado sem afetar stocks.
        /// </summary>
        [Fact]
        public async Task Remove_ProductWithNoStocks_ShouldDeactivateProduct()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Excavator XL3000",
                IsActive = true,
                Stocks = new List<Stock>()
            };

            _productRepositoryMock.Setup(x => x.GetStocksByProductId(productId))
                .ReturnsAsync(product);

            _productRepositoryMock.Setup(x => x.Update(It.IsAny<Product>()))
                .Returns(Task.CompletedTask);

            // Act
            await _productService.Remove(productId);

            // Assert
            _stockRepositoryMock.Verify(x => x.Update(It.IsAny<Stock>()), Times.Never);

            _productRepositoryMock.Verify(x => x.Update(It.Is<Product>(p =>
                p.Id == productId &&
                p.IsActive == false
            )), Times.Once);

            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Never);
        }

        /// <summary>
        /// Testa a obtenção de produtos com stock baixo.
        /// Verifica se retorna corretamente os produtos com quantidade abaixo do limiar definido (5).
        /// </summary>
        [Fact]
        public async Task GetLowStockProducts_ShouldReturnProductsWithLowStock()
        {
            // Arrange
            var lowStockProducts = new List<Product>
            {
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Product 1",
                    Quantity = 3,
                    Price = 1000.00m,
                    CategoryId = Guid.NewGuid()
                },
                new Product
                {
                    Id = Guid.NewGuid(),
                    Name = "Product 2",
                    Quantity = 4,
                    Price = 2000.00m,
                    CategoryId = Guid.NewGuid()
                }
            };

            _productRepositoryMock.Setup(x => x.GetProductsWithLowStock(5))
                .ReturnsAsync(lowStockProducts);

            // Act
            var result = await _productService.GetLowStockProducts();

            // Assert
            result.Should().HaveCount(2);
            result.Should().Contain(p => p.Name == "Product 1");
            result.Should().Contain(p => p.Name == "Product 2");
            _productRepositoryMock.Verify(x => x.GetProductsWithLowStock(5), Times.Once);
        }
    }
}