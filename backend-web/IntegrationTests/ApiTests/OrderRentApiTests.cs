using FluentAssertions;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.WebApp.ViewModels.API;
using System.Net.Http.Json;

namespace IntegrationTests.ApiTests
{
    /// <summary>
    /// Classe de testes de integração para a API de alugueres de encomendas (OrderRentApi).
    /// Testa os endpoints HTTP (GET, POST, PUT, DELETE) relacionados com o ciclo de vida dos alugueres.
    /// </summary>
    public class OrderRentApiTests : IClassFixture<IntegrationTestWebAppFactory>
    {
        /// <summary>
        /// Factory responsável por criar a aplicação web em ambiente de teste.
        /// </summary>
        private readonly IntegrationTestWebAppFactory _factory;
        /// <summary>
        /// Cliente HTTP utilizado para efetuar os pedidos à API.
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Construtor que inicializa o cliente HTTP e o factory para os testes.
        /// </summary>
        /// <param name="factory">Instância da factory de testes de integração.</param>
        public OrderRentApiTests(IntegrationTestWebAppFactory factory)
        {
            _factory = factory;
            _httpClient = _factory.CreateAuthenticatedClient();
            _factory.ResetDatabase();
        }

        #region GET Tests
        /// <summary>
        /// Testa a obtenção de todos os registos de alugueres (OrderRent).
        /// Deve retornar o código HTTP 200 (OK).
        /// </summary>
        [Fact]
        public async Task GetAll_ShouldReturnOK()
        {

            // Act
            var response = await _httpClient.GetAsync("/api/v1/orderrent");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        /// Testa a obtenção de um OrderRent existente pelo seu ID.
        /// Deve retornar o código HTTP 200 (OK).
        /// </summary>
        [Fact]
        public async Task GetById_WhenExists_ShouldReturnOK()
        {
            // Arrange
            // Buscar o ID do OrderRent existente no seeding
            var seeded = await _httpClient.GetFromJsonAsync<List<OrderRentApiViewModel>>("/api/v1/orderrent");
            var orderRentId = seeded!.First().Id;

            // Act
            var response = await _httpClient.GetAsync($"/api/v1/orderrent/{orderRentId}");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        /// Testa a obtenção de um OrderRent inexistente pelo ID.
        /// Deve retornar o código HTTP 404 (NotFound).
        /// </summary>
        [Fact]
        public async Task GetById_WhenDoesNotExist_ShouldReturnNotFound()
        {
            // Arrange
            var nonExistentId = Guid.Parse("bbbbbbbb-bbbb-bbbb-ffff-bbbbbbbbbbbb");

            // Act
            var response = await _httpClient.GetAsync($"/api/v1/orderrent/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }
        #endregion

        #region POST Tests
        /// <summary>
        /// Testa a criação de um novo OrderRent válido.
        /// Deve retornar o código HTTP 200 (OK).
        /// </summary>
        [Fact]
        public async Task Create_WhenValid_ShouldReturnOK()
        {
            // Arrange
            // Usar OrderId existente (order2 - Cancelled)
            var orderId = Guid.Parse("aaaaaaaa-cccc-aaaa-aaaa-aaaaaaaaaaaa");

            var newOrderRent = new OrderRentApiViewModel
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                InitialDate = DateTime.UtcNow.AddDays(1),
                EndDate = DateTime.UtcNow.AddDays(7),
                CompanyImage = "company.jpg",
                Status = OrderRentStatus.Approved
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync("/api/v1/orderrent", newOrderRent);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
        #endregion

        #region PUT Tests
        /// <summary>
        /// Testa a atualização de um OrderRent existente, alterando as datas de início e fim.
        /// Deve retornar o código HTTP 200 (OK).
        /// </summary>
        [Fact]
        public async Task Update_WhenChangingDates_ShouldReturnOK()
        {
            // Arrange
            var seeded = await _httpClient.GetFromJsonAsync<List<OrderRentApiViewModel>>("/api/v1/orderrent");
            var orderRent = seeded!.First();

            // Criar uma cï¿½pia para modificar
            var updatedOrderRent = new OrderRentApiViewModel
            {
                Id = orderRent.Id,
                OrderId = orderRent.OrderId,
                InitialDate = DateTime.UtcNow.AddDays(-2),
                EndDate = DateTime.UtcNow.AddDays(10),
                CompanyImage = orderRent.CompanyImage,
                UserImage = orderRent.UserImage,
                Status = orderRent.Status
            };

            // Act
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/orderrent/{updatedOrderRent.Id}", updatedOrderRent);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        /// Testa a atualização de um OrderRent, alterando o estado para Completed com imagem do utilizador.
        /// Deve retornar o código HTTP 200 (OK).
        /// </summary>
        [Fact]
        public async Task Update_WhenChangeToCompleted_WithUserImage_ShouldReturnOK()
        {
            // Arrange
            var seeded = await _httpClient.GetFromJsonAsync<List<OrderRentApiViewModel>>("/api/v1/orderrent");
            var orderRent = seeded!.First(o => o.Status == OrderRentStatus.Active);

            // Change to Completed with UserImage
            orderRent.Status = OrderRentStatus.Completed;
            orderRent.UserImage = "user-completion.jpg";
            orderRent.EndDate = DateTime.UtcNow.AddDays(-1); // Already ended

            // Act
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/orderrent/{orderRent.Id}", orderRent);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
        #endregion

        #region DELETE Tests

        /// <summary>
        /// Testa o ciclo de vida completo de um OrderRent:
        /// criação, cancelamento e eliminação.
        /// Cada passo deve retornar o código HTTP 200 (OK).
        /// </summary>
        [Fact]
        public async Task Lifecycle_CreatePendingCancelAndDelete_ShouldWorkCorrectly()
        {
            // Arrange - Create Pending using order3 (completed)
            var orderId = Guid.Parse("aaaaaaaa-cccc-aaaa-aaaa-aaaaaaaaaaaa");

            var newOrderRent = new OrderRentApiViewModel
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                InitialDate = DateTime.UtcNow.AddDays(5),
                EndDate = DateTime.UtcNow.AddDays(10),
                CompanyImage = "cancel-lifecycle.jpg",
                Status = OrderRentStatus.Approved
            };

            // Act & Assert - Create
            var createResponse = await _httpClient.PostAsJsonAsync("/api/v1/orderrent", newOrderRent);
            createResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            // Act & Assert - Cancel
            newOrderRent.Status = OrderRentStatus.Cancelled;
            var cancelResponse = await _httpClient.PutAsJsonAsync($"/api/v1/orderrent/{newOrderRent.Id}", newOrderRent);
            cancelResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            // Act & Assert - Delete (should succeed - Cancelled orders can be deleted)
            var deleteResponse = await _httpClient.DeleteAsync($"/api/v1/orderrent/{newOrderRent.Id}");
            deleteResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
        #endregion
    }
}