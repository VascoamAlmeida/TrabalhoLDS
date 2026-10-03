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
    /// Classe de testes de integração para a API de Stock.
    /// Testa os endpoints relacionados à criação, atualização, remoção e ações especiais de Stock.
    /// </summary>
    public class StockApiTests : IClassFixture<IntegrationTestWebAppFactory>
    {
        private readonly HttpClient _httpClient;
        private readonly IntegrationTestWebAppFactory _factory;
        private const string API_BASE_PATH = "/api/v1/stock";

        private readonly Guid SEEDED_PRODUCT_ID;
        private readonly Guid SEEDED_STOCK_ID;
        private readonly Guid SEEDED_STOCK_REF;

        /// <summary>
        /// Construtor da classe de testes que inicializa o cliente HTTP e a base de dados de teste.
        /// </summary>
        /// <param name="factory">Instância da fábrica de aplicação de teste.</param>
        public StockApiTests(IntegrationTestWebAppFactory factory)
        {
            _factory = factory;
            _httpClient = _factory.CreateAuthenticatedClient();

            ResetDatabase();

            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            SEEDED_PRODUCT_ID = db.E_Products.First().Id;
            SEEDED_STOCK_ID = db.E_Stocks.First().Id;
            SEEDED_STOCK_REF = db.E_Stocks.First().Ref;
        }

        /// <summary>
        /// Restaura a base de dados de teste para o estado inicial antes de cada teste.
        /// </summary>
        private void ResetDatabase()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            Seeding.InitializeTestDB(db);
        }

        /// <summary>
        /// Testa a criação de um novo Stock válido.
        /// Deve retornar status HTTP 200 OK.
        /// </summary>
        [Fact]
        public async Task CreateStock_WhenValid_ReturnOK()
        {
            // Arrange
            var newStock = new StockApiViewModel()
            {
                Id = Guid.NewGuid(),
                Ref = Guid.NewGuid(),
                ProductId = SEEDED_PRODUCT_ID,
                StockStatus = StockStatus.Available
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(API_BASE_PATH, newStock);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify creation by fetching the stock
            var fetchResponse = await _httpClient.GetAsync($"{API_BASE_PATH}/{newStock.Id}");
            fetchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Testa a criação de um Stock com uma referência duplicada.
        /// Deve retornar status HTTP 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task CreateStock_WhenDuplicateRef_ReturnBadRequest()
        {
            // Arrange
            var duplicateStock = new StockApiViewModel()
            {
                Id = Guid.NewGuid(),
                Ref = SEEDED_STOCK_REF,
                ProductId = SEEDED_PRODUCT_ID,
                StockStatus = StockStatus.Available
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(API_BASE_PATH, duplicateStock);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Testa a criação de um Stock com dados inválidos.
        /// Deve retornar status HTTP 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task CreateStock_WhenValidationFails_ReturnBadRequest()
        {
            // Arrange
            var invalidStock = new StockApiViewModel()
            {
                Id = Guid.NewGuid(),
                Ref = Guid.Empty,
                ProductId = Guid.Empty,
                StockStatus = (StockStatus)999
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(API_BASE_PATH, invalidStock);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Testa a criação de um Stock com estado "SoldOut".
        /// Deve retornar status HTTP 400 BadRequest, pois não é permitido criar já vendido.
        /// </summary>
        [Fact]
        public async Task CreateStock_WithSoldOutStatus_ReturnBadRequest()
        {
            // Arrange
            var invalidStock = new StockApiViewModel()
            {
                Id = Guid.NewGuid(),
                Ref = Guid.NewGuid(),
                ProductId = SEEDED_PRODUCT_ID,
                StockStatus = StockStatus.SoldOut
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(API_BASE_PATH, invalidStock);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Testa a atualização de um Stock existente com dados válidos.
        /// Deve retornar status HTTP 200 OK.
        /// </summary>
        [Fact]
        public async Task UpdateStock_WhenValid_ReturnOK()
        {
            // Arrange
            var newStockId = Guid.NewGuid();
            var newStock = new StockApiViewModel()
            {
                Id = newStockId,
                Ref = Guid.NewGuid(),
                ProductId = SEEDED_PRODUCT_ID,
                StockStatus = StockStatus.Available
            };

            var createResponse = await _httpClient.PostAsJsonAsync(API_BASE_PATH, newStock);
            createResponse.EnsureSuccessStatusCode();

            var updatedRef = Guid.NewGuid();
            newStock.Ref = updatedRef;
            newStock.StockStatus = StockStatus.NotAvailable;

            // Act
            var response = await _httpClient.PutAsJsonAsync($"{API_BASE_PATH}/{newStockId}", newStock);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify the update
            var fetchResponse = await _httpClient.GetAsync($"{API_BASE_PATH}/{newStockId}");
            fetchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedStock = await fetchResponse.Content.ReadFromJsonAsync<StockApiViewModel>();
            updatedStock!.Ref.Should().Be(updatedRef);
            updatedStock.StockStatus.Should().Be(StockStatus.NotAvailable);
        }

        /// <summary>
        /// Testa a atualização de um Stock quando o ID do caminho não corresponde ao do corpo.
        /// Deve retornar status HTTP 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateStock_WhenIDMismatch_ReturnBadRequest()
        {
            // Arrange
            var validStock = new StockApiViewModel()
            {
                Id = Guid.NewGuid(),
                Ref = Guid.NewGuid(),
                ProductId = SEEDED_PRODUCT_ID,
                StockStatus = StockStatus.Available
            };
            var pathId = Guid.NewGuid();

            // Act
            var response = await _httpClient.PutAsJsonAsync($"{API_BASE_PATH}/{pathId}", validStock);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Testa a atualização de um Stock usando uma referência já existente em outro Stock.
        /// Deve retornar status HTTP 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateStock_WhenDuplicateRef_ReturnBadRequest()
        {
            // Arrange
            var stock1Id = Guid.NewGuid();
            var stock1 = new StockApiViewModel()
            {
                Id = stock1Id,
                Ref = Guid.NewGuid(),
                ProductId = SEEDED_PRODUCT_ID,
                StockStatus = StockStatus.Available
            };

            var stock2Id = Guid.NewGuid();
            var stock2 = new StockApiViewModel()
            {
                Id = stock2Id,
                Ref = Guid.NewGuid(),
                ProductId = SEEDED_PRODUCT_ID,
                StockStatus = StockStatus.Available
            };

            var createResponse1 = await _httpClient.PostAsJsonAsync(API_BASE_PATH, stock1);
            createResponse1.EnsureSuccessStatusCode();

            var createResponse2 = await _httpClient.PostAsJsonAsync(API_BASE_PATH, stock2);
            createResponse2.EnsureSuccessStatusCode();

            stock1.Ref = stock2.Ref;

            // Act
            var response = await _httpClient.PutAsJsonAsync($"{API_BASE_PATH}/{stock1Id}", stock1);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Testa a exclusão de um Stock existente e disponível.
        /// Deve retornar status HTTP 200 OK e o recurso deve ser removido.
        /// </summary>
        [Fact]
        public async Task DeleteStock_WhenAvailable_ReturnOK()
        {
            // Arrange
            var stockIdToDelete = Guid.NewGuid();
            var stockToDelete = new StockApiViewModel()
            {
                Id = stockIdToDelete,
                Ref = Guid.NewGuid(),
                ProductId = SEEDED_PRODUCT_ID,
                StockStatus = StockStatus.Available
            };

            var createResponse = await _httpClient.PostAsJsonAsync(API_BASE_PATH, stockToDelete);
            createResponse.EnsureSuccessStatusCode();

            // Act
            var response = await _httpClient.DeleteAsync($"{API_BASE_PATH}/{stockIdToDelete}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify deletion
            var checkResponse = await _httpClient.GetAsync($"{API_BASE_PATH}/{stockIdToDelete}");
            checkResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Testa a exclusão de um Stock inexistente.
        /// Deve retornar status HTTP 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeleteStock_WhenDoesNotExist_ReturnNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _httpClient.DeleteAsync($"{API_BASE_PATH}/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Testa a operação de retorno de aluguer em um Stock que já está disponível.
        /// Deve retornar status HTTP 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task ReturnRent_WhenAlreadyAvailable_ReturnBadRequest()
        {
            // Arrange
            // Act
            var response = await _httpClient.PostAsync($"{API_BASE_PATH}/{SEEDED_STOCK_ID}/return-rent", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // <summary>
        /// Testa a operação de retorno de aluguer em um Stock inexistente.
        /// Deve retornar status HTTP 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task ReturnRent_WhenDoesNotExist_ReturnBadRequest()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _httpClient.PostAsync($"{API_BASE_PATH}/{nonExistentId}/return-rent", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Testa o cancelamento de devolução em um Stock que já está disponível.
        /// Deve retornar status HTTP 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task ReturnCancel_WhenAlreadyAvailable_ReturnBadRequest()
        {
            // Act
            var response = await _httpClient.PostAsync($"{API_BASE_PATH}/{SEEDED_STOCK_ID}/return-cancel", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Testa o cancelamento de devolução em um Stock inexistente.
        /// Deve retornar status HTTP 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task ReturnCancel_WhenDoesNotExist_ReturnBadRequest()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _httpClient.PostAsync($"{API_BASE_PATH}/{nonExistentId}/return-cancel", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Testa a tentativa de atualizar o estado de um Stock para "SoldOut" sem pedidos associados.
        /// Deve retornar status HTTP 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateStock_ToSoldOutWithoutOrders_ReturnBadRequest()
        {
            // Arrange
            var newStockId = Guid.NewGuid();
            var newStock = new StockApiViewModel()
            {
                Id = newStockId,
                Ref = Guid.NewGuid(),
                ProductId = SEEDED_PRODUCT_ID,
                StockStatus = StockStatus.Available
            };

            var createResponse = await _httpClient.PostAsJsonAsync(API_BASE_PATH, newStock);
            createResponse.EnsureSuccessStatusCode();

            newStock.StockStatus = StockStatus.SoldOut;

            // Act
            var response = await _httpClient.PutAsJsonAsync($"{API_BASE_PATH}/{newStockId}", newStock);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }
}