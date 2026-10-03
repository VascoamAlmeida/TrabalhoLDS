using FluentAssertions;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Data.Contexts;
using LojaMaquinas.WebApp.ViewModels.API;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests.ApiTests
{
    /// <summary>
    /// Classe de testes de integração para a API de manutenções.
    /// Testa os endpoints HTTP (GET, POST, PUT, DELETE) da MaintenanceApi.
    /// </summary>
    public class MaintenanceApiTests : IClassFixture<IntegrationTestWebAppFactory>
    {
        /// <summary>
        /// Cliente HTTP para fazer pedidos à API.
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Factory para criar a aplicação web de testes.
        /// </summary>
        private readonly IntegrationTestWebAppFactory _factory;

        /// <summary>
        /// Caminho base da API de manutenções.
        /// </summary>
        private const string API_BASE_PATH = "/api/MaintenanceApi";

        /// <summary>
        /// ID do pedido 1 pré-carregado nos testes.
        /// </summary>
        private readonly Guid SEEDED_ORDER_ID_1 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

        /// <summary>
        /// ID do pedido 2 pré-carregado nos testes.
        /// </summary>
        private readonly Guid SEEDED_ORDER_ID_2 = Guid.Parse("aaaaaaaa-bbbb-aaaa-aaaa-aaaaaaaaaaaa");

        /// <summary>
        /// ID do pedido 3 pré-carregado nos testes.
        /// </summary>
        private readonly Guid SEEDED_ORDER_ID_3 = Guid.Parse("aaaaaaaa-cccc-aaaa-aaaa-aaaaaaaaaaaa");

        /// <summary>
        /// Construtor que inicializa o cliente HTTP e reinicia a base de dados para cada teste.
        /// </summary>
        /// <param name="factory">Factory da aplicação web de testes</param>
        public MaintenanceApiTests(IntegrationTestWebAppFactory factory)
        {
            _factory = factory;
            _httpClient = _factory.CreateAuthenticatedClient();

            // Reinicia a base de dados antes de cada teste
            ResetDatabase();
        }

        /// <summary>
        /// Reinicia a base de dados para o estado inicial dos testes.
        /// </summary>
        private void ResetDatabase()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            Seeding.InitializeTestDB(db);
        }

        // --- POST (CREATE) TESTS ---

        /// <summary>
        /// Testa a criação de uma manutenção válida.
        /// Deve retornar o código HTTP 200 (OK).
        /// </summary>
        [Fact]
        public async Task CreateMaintenance_WhenValid_ReturnOK()
        {
            // Arrange
            var newMaintenance = new MaintenanceApiViewModel()
            {
                Id = Guid.NewGuid(),
                OrderId = SEEDED_ORDER_ID_1,
                ScheduledDate = DateTime.UtcNow.Date.AddDays(2),
                Price = 150.50m,
                Obs = "Routine maintenance check",
                Status = MaintenanceStatus.Pending
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(API_BASE_PATH, newMaintenance);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Create failed: {errorContent}");
            }

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Testa a criação de uma manutenção com um OrderId inválido.
        /// Deve retornar BadRequest ou InternalServerError.
        /// </summary>
        [Fact]
        public async Task CreateMaintenance_WhenInvalidOrderId_ReturnBadRequest()
        {
            // Arrange
            var invalidMaintenance = new MaintenanceApiViewModel()
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(), // Non-existent order
                ScheduledDate = DateTime.UtcNow.Date,
                Price = 100.00m,
                Obs = "Test maintenance",
                Status = MaintenanceStatus.Pending
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(API_BASE_PATH, invalidMaintenance);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Testa a criação de uma manutenção com falhas de validação.
        /// Deve retornar BadRequest.
        /// </summary>
        [Fact]
        public async Task CreateMaintenance_WhenValidationFails_ReturnBadRequest()
        {
            // Arrange
            var invalidMaintenance = new MaintenanceApiViewModel()
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.Empty,
                ScheduledDate = DateTime.MinValue,
                Price = -100.00m,
                Obs = "",
                Status = (MaintenanceStatus)999
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(API_BASE_PATH, invalidMaintenance);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Testa a criação de uma manutenção com um ID já existente.
        /// Deve retornar BadRequest ou InternalServerError.
        /// </summary>
        [Fact]
        public async Task CreateMaintenance_WhenDuplicateId_ReturnBadRequest()
        {
            // Arrange - Get existing seeded maintenance ID
            var maintenancesResponse = await _httpClient.GetAsync(API_BASE_PATH);
            maintenancesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var maintenances = await maintenancesResponse.Content.ReadFromJsonAsync<IEnumerable<MaintenanceApiViewModel>>();
            var existingMaintenanceId = maintenances!.First().Id;

            // Try to create another maintenance with same ID as existing seeded maintenance
            var duplicateMaintenance = new MaintenanceApiViewModel()
            {
                Id = existingMaintenanceId, // Same ID as seeded maintenance
                OrderId = SEEDED_ORDER_ID_1,
                ScheduledDate = DateTime.UtcNow.Date.AddDays(8),
                Price = 200.00m,
                Obs = "Duplicate maintenance",
                Status = MaintenanceStatus.Pending
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(API_BASE_PATH, duplicateMaintenance);

            // Assert
            // This might return BadRequest or 500 depending on how duplicates are handled
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        // --- PUT (UPDATE) TESTS ---
        /// <summary>
        /// Testa a atualização de uma manutenção existente com dados válidos.
        /// Deve retornar o código HTTP 200 (OK).
        /// </summary>
        [Fact]
        public async Task UpdateMaintenance_WhenValid_ReturnOK()
        {
            // Arrange
            var maintenancesResponse = await _httpClient.GetAsync(API_BASE_PATH);
            maintenancesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var maintenances = await maintenancesResponse.Content.ReadFromJsonAsync<IEnumerable<MaintenanceApiViewModel>>();
            var existingMaintenance = maintenances!.First();

            var updatedObs = "Updated maintenance observation after inspection";
            var updatedCost = 275.50m;
            var updatedStatus = MaintenanceStatus.InProgress;
            var updatedDate = DateTime.UtcNow.Date.AddDays(4);

            existingMaintenance.Obs = updatedObs;
            existingMaintenance.Price = updatedCost;
            existingMaintenance.Status = updatedStatus;
            existingMaintenance.ScheduledDate = updatedDate;

            // Act
            var response = await _httpClient.PutAsJsonAsync($"{API_BASE_PATH}/{existingMaintenance.Id}", existingMaintenance);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var fetchResponse = await _httpClient.GetAsync($"{API_BASE_PATH}/{existingMaintenance.Id}");
            fetchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedMaintenance = await fetchResponse.Content.ReadFromJsonAsync<MaintenanceApiViewModel>();
            updatedMaintenance!.Obs.Should().Be(updatedObs);
            updatedMaintenance.Price.Should().Be(updatedCost);
            updatedMaintenance.Status.Should().Be(updatedStatus);
            updatedMaintenance.ScheduledDate.Should().Be(updatedDate);
        }

        /// <summary>
        /// Testa a atualização de uma manutenção em que o ID do objeto
        /// não coincide com o ID passado na rota. Deve retornar BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateMaintenance_WhenIDMismatch_ReturnBadRequest()
        {
            // Arrange
            var validMaintenance = new MaintenanceApiViewModel()
            {
                Id = Guid.NewGuid(),
                OrderId = SEEDED_ORDER_ID_1,
                ScheduledDate = DateTime.UtcNow.Date,
                Price = 100.00m,
                Obs = "Test Maintenance",
                Status = MaintenanceStatus.Pending
            };
            var pathId = Guid.NewGuid();

            // Act
            var response = await _httpClient.PutAsJsonAsync($"{API_BASE_PATH}/{pathId}", validMaintenance);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Testa a atualização de uma manutenção inexistente.
        /// Deve retornar BadRequest ou InternalServerError.
        /// </summary>
        [Fact]
        public async Task UpdateMaintenance_WhenDoesNotExist_ReturnBadRequest()
        {
            // Arrange
            var nonExistentMaintenance = new MaintenanceApiViewModel()
            {
                Id = Guid.NewGuid(),
                OrderId = SEEDED_ORDER_ID_1,
                ScheduledDate = DateTime.UtcNow.Date,
                Price = 100.00m,
                Obs = "Non-existent maintenance",
                Status = MaintenanceStatus.Pending
            };

            // Act
            var response = await _httpClient.PutAsJsonAsync($"{API_BASE_PATH}/{nonExistentMaintenance.Id}", nonExistentMaintenance);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.InternalServerError);
        }

        // --- DELETE TESTS ---

        /// <summary>
        /// Testa a exclusão de uma manutenção existente.
        /// Deve retornar o código HTTP 200 (OK) e a manutenção
        /// deve deixar de estar acessível posteriormente.
        /// </summary>
        [Fact]
        public async Task DeleteMaintenance_WhenExists_ReturnOK()
        {
            // Arrange
            var maintenancesResponse = await _httpClient.GetAsync(API_BASE_PATH);
            maintenancesResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var maintenances = await maintenancesResponse.Content.ReadFromJsonAsync<IEnumerable<MaintenanceApiViewModel>>();
            var maintenanceToDelete = maintenances!.First();

            // Act
            var response = await _httpClient.DeleteAsync($"{API_BASE_PATH}/{maintenanceToDelete.Id}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var checkResponse = await _httpClient.GetAsync($"{API_BASE_PATH}/{maintenanceToDelete.Id}");
            checkResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Testa a tentativa de exclusão de uma manutenção que não existe.
        /// Deve retornar o código HTTP 404 (NotFound).
        /// </summary>
        [Fact]
        public async Task DeleteMaintenance_WhenDoesNotExist_ReturnNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _httpClient.DeleteAsync($"{API_BASE_PATH}/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }                
    }
}