using FluentAssertions;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Data.Contexts;
using LojaMaquinas.WebApp.ViewModels.MVC;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests.ApiTests
{
    /// <summary>
    /// Classe de testes de integração para a API de produtos.
    /// Testa as operações de CRUD e endpoints específicos do ProductApi.
    /// </summary>
    public class ProductApiTests : IClassFixture<IntegrationTestWebAppFactory>
    {
        private readonly HttpClient _httpClient;
        private readonly IntegrationTestWebAppFactory _factory;
        private const string API_BASE_PATH = "/api/v1/products";

        private readonly Guid SEEDED_CATEGORY_ID;
        private readonly Guid SEEDED_PRODUCT_ID;

        /// <summary>
        /// Construtor da classe de testes de integração de produtos.
        /// Inicializa o HttpClient e os dados de seed para as categorias e produtos.
        /// </summary>
        /// <param name="factory">Factory responsável por criar o ambiente de teste da aplicação.</param>
        public ProductApiTests(IntegrationTestWebAppFactory factory)
        {
            _factory = factory;
            _httpClient = _factory.CreateAuthenticatedClient();

            ResetDatabase();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            SEEDED_CATEGORY_ID = db.P_Categories.First().Id;
            SEEDED_PRODUCT_ID = db.E_Products.First().Id;
        }

        /// <summary>
        /// Recria a base de dados em memória com os dados iniciais de seed antes de cada execução de teste.
        /// </summary>
        private void ResetDatabase()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            Seeding.InitializeTestDB(db);
        }

        /// <summary>
        /// Testa a criação de um produto válido.
        /// Deve retornar 200 OK e o produto deve poder ser recuperado via GET.
        /// </summary>
        [Fact]
        public async Task CreateProduct_WhenValid_ReturnOK()
        {
            // Arrange
            var newProduct = new ProductApiViewModel()
            {
                Id = Guid.NewGuid(),
                Name = "New Test Machine",
                Type = ProductType.Machine,
                Brand = "TestBrand",
                Model = "TestModel-X100",
                YearManufacture = 2024,
                Description = "This is a test product description for integration testing",
                Price = 15000.00m,
                Quantity = 10,
                Image = "test-image.png",
                IsActive = true,
                CategoryId = SEEDED_CATEGORY_ID
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(API_BASE_PATH, newProduct);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify creation by fetching the product
            var fetchResponse = await _httpClient.GetAsync($"{API_BASE_PATH}/{newProduct.Id}");
            fetchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Testa a tentativa de criação de um produto com um ID duplicado.
        /// Deve retornar 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task CreateProduct_WhenDuplicateId_ReturnBadRequest()
        {
            // Arrange
            var duplicateProduct = new ProductApiViewModel()
            {
                Id = SEEDED_PRODUCT_ID,
                Name = "Duplicate ID Product",
                Type = ProductType.Machine,
                Brand = "TestBrand",
                Model = "TestModel",
                YearManufacture = 2024,
                Description = "Test description",
                Price = 10000.00m,
                Quantity = 5,
                Image = "test.png",
                IsActive = true,
                CategoryId = SEEDED_CATEGORY_ID
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(API_BASE_PATH, duplicateProduct);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Testa a tentativa de criação de um produto inválido (erros de validação).
        /// Deve retornar 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task CreateProduct_WhenValidationFails_ReturnBadRequest()
        {
            // Arrange
            var invalidProduct = new ProductApiViewModel()
            {
                Id = Guid.NewGuid(),
                Name = "",
                Type = (ProductType)999,
                Brand = "",
                Model = "",
                YearManufacture = 1800,
                Description = "Too short",
                Price = -100.00m,
                Quantity = -5,
                Image = "",
                IsActive = true,
                CategoryId = Guid.Empty
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(API_BASE_PATH, invalidProduct);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Testa a criação de produtos com diferentes tipos (Machine e Part).
        /// Deve retornar 200 OK para ambos.
        /// </summary>
        [Fact]
        public async Task CreateProduct_WithDifferentTypes_ReturnOK()
        {
            // Arrange
            var productTypes = new[]
            {
                ProductType.Machine,
                ProductType.Part
            };

            foreach (var productType in productTypes)
            {
                var product = new ProductApiViewModel()
                {
                    Id = Guid.NewGuid(),
                    Name = $"Test {productType}",
                    Type = productType,
                    Brand = "TestBrand",
                    Model = $"Model-{productType}",
                    YearManufacture = 2024,
                    Description = $"Test description for {productType}",
                    Price = productType == ProductType.Machine ? 20000.00m : 500.00m,
                    Quantity = 10,
                    Image = "test.png",
                    IsActive = true,
                    CategoryId = SEEDED_CATEGORY_ID
                };

                // Act
                var response = await _httpClient.PostAsJsonAsync(API_BASE_PATH, product);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.OK, $"should create product with type {productType}");

                // Verify creation
                var fetchResponse = await _httpClient.GetAsync($"{API_BASE_PATH}/{product.Id}");
                fetchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                var createdProduct = await fetchResponse.Content.ReadFromJsonAsync<ProductApiViewModel>();
                createdProduct!.Type.Should().Be(productType);
            }
        }

        /// <summary>
        /// Testa a atualização de um produto existente com dados válidos.
        /// Deve retornar 200 OK e refletir as alterações.
        /// </summary>
        [Fact]
        public async Task UpdateProduct_WhenValid_ReturnOK()
        {
            // Arrange
            var newProductId = Guid.NewGuid();
            var newProduct = new ProductApiViewModel()
            {
                Id = newProductId,
                Name = "Original Product Name",
                Type = ProductType.Machine,
                Brand = "OriginalBrand",
                Model = "OriginalModel",
                YearManufacture = 2023,
                Description = "Original description",
                Price = 10000.00m,
                Quantity = 5,
                Image = "original.png",
                IsActive = true,
                CategoryId = SEEDED_CATEGORY_ID
            };

            var createResponse = await _httpClient.PostAsJsonAsync(API_BASE_PATH, newProduct);
            createResponse.EnsureSuccessStatusCode();

            var updatedName = "Updated Product Name";
            var updatedPrice = 12000.00m;
            var updatedDescription = "Updated product description after changes";

            newProduct.Name = updatedName;
            newProduct.Price = updatedPrice;
            newProduct.Description = updatedDescription;
            newProduct.YearManufacture = 2024;

            // Act
            var response = await _httpClient.PutAsJsonAsync($"{API_BASE_PATH}/{newProductId}", newProduct);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify the update
            var fetchResponse = await _httpClient.GetAsync($"{API_BASE_PATH}/{newProductId}");
            fetchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedProduct = await fetchResponse.Content.ReadFromJsonAsync<ProductApiViewModel>();
            updatedProduct!.Name.Should().Be(updatedName);
            updatedProduct.Price.Should().Be(updatedPrice);
            updatedProduct.Description.Should().Be(updatedDescription);
            updatedProduct.YearManufacture.Should().Be(2024);
        }

        /// <summary>
        /// Testa a tentativa de atualização de um produto cujo ID do caminho não corresponde ao do corpo.
        /// Deve retornar 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateProduct_WhenIDMismatch_ReturnBadRequest()
        {
            // Arrange
            var validProduct = new ProductApiViewModel()
            {
                Id = Guid.NewGuid(),
                Name = "Test Product",
                Type = ProductType.Machine,
                Brand = "TestBrand",
                Model = "TestModel",
                YearManufacture = 2024,
                Description = "Test description",
                Price = 10000.00m,
                Quantity = 5,
                Image = "test.png",
                IsActive = true,
                CategoryId = SEEDED_CATEGORY_ID
            };
            var pathId = Guid.NewGuid();

            // Act
            var response = await _httpClient.PutAsJsonAsync($"{API_BASE_PATH}/{pathId}", validProduct);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Testa a tentativa de atualização de um produto inexistente.
        /// Deve retornar 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateProduct_WhenDoesNotExist_ReturnBadRequest()
        {
            // Arrange
            var nonExistentProduct = new ProductApiViewModel()
            {
                Id = Guid.NewGuid(),
                Name = "Non-existent Product",
                Type = ProductType.Machine,
                Brand = "TestBrand",
                Model = "TestModel",
                YearManufacture = 2024,
                Description = "Test description",
                Price = 10000.00m,
                Quantity = 5,
                Image = "test.png",
                IsActive = true,
                CategoryId = SEEDED_CATEGORY_ID
            };

            // Act
            var response = await _httpClient.PutAsJsonAsync($"{API_BASE_PATH}/{nonExistentProduct.Id}", nonExistentProduct);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Testa a eliminação de um produto existente.
        /// Deve retornar 200 OK e marcar o produto como inativo.
        /// </summary>
        [Fact]
        public async Task DeleteProduct_WhenExists_ReturnOK()
        {
            // Arrange
            var productIdToDelete = Guid.NewGuid();
            var productToDelete = new ProductApiViewModel()
            {
                Id = productIdToDelete,
                Name = "Product to Delete",
                Type = ProductType.Machine,
                Brand = "TestBrand",
                Model = "DeleteModel",
                YearManufacture = 2024,
                Description = "This product will be deleted",
                Price = 8000.00m,
                Quantity = 3,
                Image = "delete.png",
                IsActive = true,
                CategoryId = SEEDED_CATEGORY_ID
            };

            var createResponse = await _httpClient.PostAsJsonAsync(API_BASE_PATH, productToDelete);
            createResponse.EnsureSuccessStatusCode();

            // Act
            var response = await _httpClient.DeleteAsync($"{API_BASE_PATH}/{productIdToDelete}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify the product is marked as inactive
            var fetchResponse = await _httpClient.GetAsync($"{API_BASE_PATH}/{productIdToDelete}");
            if (fetchResponse.StatusCode == HttpStatusCode.OK)
            {
                var deletedProduct = await fetchResponse.Content.ReadFromJsonAsync<ProductApiViewModel>();
                deletedProduct!.IsActive.Should().BeFalse();
            }
        }

        /// <summary>
        /// Testa a tentativa de eliminação de um produto inexistente.
        /// Deve retornar 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeleteProduct_WhenDoesNotExist_ReturnNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _httpClient.DeleteAsync($"{API_BASE_PATH}/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Testa o endpoint que devolve produtos com baixo stock.
        /// Deve retornar 200 OK e uma lista de produtos.
        /// </summary>
        [Fact]
        public async Task LowStockAlert_WhenCalled_ReturnOK()
        {
            // Arrange & Act
            var response = await _httpClient.GetAsync($"{API_BASE_PATH}/low-stock-products");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Should return products
            var products = await response.Content.ReadFromJsonAsync<IEnumerable<ProductApiViewModel>>();
            products.Should().NotBeNull();
        }

        /// <summary>
        /// Testa a atualização da quantidade de um produto para zero.
        /// Deve retornar 200 OK e refletir a alteração na base de dados.
        /// </summary>
        [Fact]
        public async Task UpdateProduct_QuantityToZero_ReturnOK()
        {
            // Arrange - Create a product first
            var productId = Guid.NewGuid();
            var product = new ProductApiViewModel()
            {
                Id = productId,
                Name = "Product for Quantity Test",
                Type = ProductType.Machine,
                Brand = "TestBrand",
                Model = "QuantityModel",
                YearManufacture = 2024,
                Description = "Testing quantity update to zero",
                Price = 5000.00m,
                Quantity = 10,
                Image = "test.png",
                IsActive = true,
                CategoryId = SEEDED_CATEGORY_ID
            };

            var createResponse = await _httpClient.PostAsJsonAsync(API_BASE_PATH, product);
            createResponse.EnsureSuccessStatusCode();

            product.Quantity = 0;

            // Act
            var response = await _httpClient.PutAsJsonAsync($"{API_BASE_PATH}/{productId}", product);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify quantity update
            var fetchResponse = await _httpClient.GetAsync($"{API_BASE_PATH}/{productId}");
            var updatedProduct = await fetchResponse.Content.ReadFromJsonAsync<ProductApiViewModel>();
            updatedProduct!.Quantity.Should().Be(0);
        }
    }
}