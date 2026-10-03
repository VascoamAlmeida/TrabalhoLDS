using FluentAssertions;
using LojaMaquinas.Data.Contexts;
using LojaMaquinas.WebApp.ViewModels.API;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests.ApiTests
{
    /// <summary>
    /// Classe de testes de integração para a API de clientes.
    /// Testa os endpoints HTTP (GET, POST, PUT, DELETE) da ClientApi.
    /// </summary>
    public class ClientApiTests : IClassFixture<IntegrationTestWebAppFactory>
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
        /// Caminho base da API de clientes.
        /// </summary>
        private const string API_BASE_PATH = "/api/ClientApi";

        /// <summary>
        /// Primeiro nome do cliente pré-carregado nos testes.
        /// </summary>
        private const string SEEDED_CLIENT_FIRST_NAME = "Client";

        /// <summary>
        /// Último nome do cliente pré-carregado nos testes.
        /// </summary>
        private const string SEEDED_CLIENT_LAST_NAME = "Client";

        /// <summary>
        /// NIF do cliente pré-carregado nos testes.
        /// </summary>
        private const string SEEDED_CLIENT_NIF = "123456789";

        /// <summary>
        /// Morada do cliente pré-carregado nos testes.
        /// </summary>
        private const string SEEDED_CLIENT_ADDRESS = "Client Street";

        /// <summary>
        /// Código postal do cliente pré-carregado nos testes.
        /// </summary>
        private const string SEEDED_CLIENT_POSTAL_CODE = "1234-567";

        /// <summary>
        /// Telefone do cliente pré-carregado nos testes.
        /// </summary>
        private const string SEEDED_CLIENT_PHONE = "911111111";

        /// <summary>
        /// Construtor que inicializa o cliente HTTP e reinicia a base de dados para cada teste.
        /// </summary>
        /// <param name="factory">Factory da aplicação web de testes</param>
        public ClientApiTests(IntegrationTestWebAppFactory factory)
        {
            _factory = factory;
            _httpClient = _factory.CreateAuthenticatedClient();

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

        // --- GET TESTS ---

        /// <summary>
        /// Testa o endpoint GET que retorna todos os clientes.
        /// Verifica se retorna status 200 OK e contém o cliente pré-carregado.
        /// </summary>
        [Fact]
        public async Task GetAllClients_ReturnOK()
        {
            // Act
            var response = await _httpClient.GetAsync(API_BASE_PATH);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var clients = await response.Content.ReadFromJsonAsync<IEnumerable<ClientApiViewModel>>();
            clients.Should().NotBeNullOrEmpty();
            clients.Should().Contain(c => c.NIF == SEEDED_CLIENT_NIF);
        }

        /// <summary>
        /// Testa o endpoint GET que retorna um cliente por ID.
        /// Verifica se retorna status 200 OK quando o cliente existe.
        /// </summary>
        [Fact]
        public async Task GetClientById_WhenExists_ReturnOK()
        {
            // Arrange
            var allClients = await _httpClient.GetFromJsonAsync<IEnumerable<ClientApiViewModel>>(API_BASE_PATH);
            var clientId = allClients!.First(c => c.NIF == SEEDED_CLIENT_NIF).Id;

            // Act
            var response = await _httpClient.GetAsync($"{API_BASE_PATH}/{clientId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var client = await response.Content.ReadFromJsonAsync<ClientApiViewModel>();
            client.Should().NotBeNull();
            client!.NIF.Should().Be(SEEDED_CLIENT_NIF);
            client.FirstName.Should().Be(SEEDED_CLIENT_FIRST_NAME);
        }

        /// <summary>
        /// Testa o endpoint GET que retorna um cliente por ID inexistente.
        /// Verifica se retorna status 404 Not Found.
        /// </summary>
        [Fact]
        public async Task GetClientById_WhenDoesNotExist_ReturnNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _httpClient.GetAsync($"{API_BASE_PATH}/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Testa o endpoint GET que retorna um cliente por NIF.
        /// Verifica se retorna status 200 OK quando o cliente existe.
        /// </summary>
        [Fact]
        public async Task GetClientByNif_WhenExists_ReturnOK()
        {
            // Act
            var response = await _httpClient.GetAsync($"{API_BASE_PATH}/nif/{SEEDED_CLIENT_NIF}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var client = await response.Content.ReadFromJsonAsync<ClientApiViewModel>();
            client.Should().NotBeNull();
            client!.NIF.Should().Be(SEEDED_CLIENT_NIF);
            client.FirstName.Should().Be(SEEDED_CLIENT_FIRST_NAME);
        }

        /// <summary>
        /// Testa o endpoint GET que retorna um cliente por NIF inexistente.
        /// Verifica se retorna status 404 Not Found.
        /// </summary>
        [Fact]
        public async Task GetClientByNif_WhenDoesNotExist_ReturnNotFound()
        {
            // Arrange
            var nonExistentNif = "999999999";

            // Act
            var response = await _httpClient.GetAsync($"{API_BASE_PATH}/nif/{nonExistentNif}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Testa o endpoint GET que retorna um cliente por ID do utilizador.
        /// Verifica se retorna status 200 OK quando o cliente existe.
        /// </summary>
        [Fact]
        public async Task GetClientByUserId_WhenExists_ReturnOK()
        {
            // Arrange
            var usersResponse = await _httpClient.GetAsync("/api/UserApi");
            usersResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var users = await usersResponse.Content.ReadFromJsonAsync<IEnumerable<UserApiViewModel>>();
            var userId = users!.First(u => u.Email == "admin@mail.com").Id;

            // Act
            var response = await _httpClient.GetAsync($"{API_BASE_PATH}/user/{userId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var client = await response.Content.ReadFromJsonAsync<ClientApiViewModel>();
            client.Should().NotBeNull();
            client!.UserId.Should().Be(userId);
        }

        /// <summary>
        /// Testa o endpoint GET que retorna um cliente por ID do utilizador inexistente.
        /// Verifica se retorna status 404 Not Found.
        /// </summary>
        [Fact]
        public async Task GetClientByUserId_WhenDoesNotExist_ReturnNotFound()
        {
            // Arrange
            var nonExistentUserId = Guid.NewGuid();

            // Act
            var response = await _httpClient.GetAsync($"{API_BASE_PATH}/user/{nonExistentUserId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // --- POST (CREATE) TESTS ---

        /// <summary>
        /// Testa o endpoint POST que cria um novo cliente.
        /// Verifica se retorna status 200 OK quando os dados são válidos.
        /// </summary>
        [Fact]
        public async Task CreateClient_WhenValid_ReturnOK()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var newUser = new UserApiViewModel()
            {
                Id = userId,
                Email = "clientuser@mail.com",
                Password = "securePassword123!",
                UserType = "CLIENT",
                IsActive = true
            };

            var userResponse = await _httpClient.PostAsJsonAsync("/api/UserApi", newUser);
            userResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var newClient = new ClientApiViewModel()
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FirstName = "John",
                LastName = "Doe",
                Address = "123 Main St",
                PostalCode = "1000-001",
                PhoneNumber = "912345678",
                NIF = "987654321"
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(API_BASE_PATH, newClient);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify creation
            var fetchResponse = await _httpClient.GetAsync($"{API_BASE_PATH}/nif/{newClient.NIF}");
            fetchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Testa o endpoint POST que tenta criar um cliente com NIF duplicado.
        /// Verifica se retorna status 400 Bad Request.
        /// </summary>
        [Fact]
        public async Task CreateClient_WhenDuplicateNif_ReturnBadRequest()
        {
            // Arrange
            var duplicateClient = new ClientApiViewModel()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FirstName = "Duplicate",
                LastName = "Client",
                Address = "456 Another St",
                PostalCode = "2000-002",
                PhoneNumber = "913333333",
                NIF = SEEDED_CLIENT_NIF // Using existing NIF
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(API_BASE_PATH, duplicateClient);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Testa o endpoint POST que tenta criar um cliente com dados inválidos.
        /// Verifica se retorna status 400 Bad Request quando a validação falha.
        /// </summary>
        [Fact]
        public async Task CreateClient_WhenValidationFails_ReturnBadRequest()
        {
            // Arrange
            // Invalid client
            var invalidClient = new ClientApiViewModel()
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FirstName = "",
                LastName = "",
                Address = "Test",
                PostalCode = "invalid",
                PhoneNumber = "123",
                NIF = "abc"
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(API_BASE_PATH, invalidClient);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // --- PUT (UPDATE) TESTS ---

        /// <summary>
        /// Testa o endpoint PUT que atualiza um cliente existente.
        /// Verifica se retorna status 200 OK quando os dados são válidos.
        /// </summary>
        [Fact]
        public async Task UpdateClient_WhenValid_ReturnOK()
        {
            // Arrange
            var allClients = await _httpClient.GetFromJsonAsync<IEnumerable<ClientApiViewModel>>(API_BASE_PATH);
            var clientToUpdate = allClients!.First(c => c.NIF == SEEDED_CLIENT_NIF);

            var updatedFirstName = "UpdatedFirst";
            var updatedLastName = "UpdatedLast";
            clientToUpdate.FirstName = updatedFirstName;
            clientToUpdate.LastName = updatedLastName;

            // Act
            var response = await _httpClient.PutAsJsonAsync($"{API_BASE_PATH}/{clientToUpdate.Id}", clientToUpdate);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify the update
            var fetchResponse = await _httpClient.GetAsync($"{API_BASE_PATH}/{clientToUpdate.Id}");
            fetchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            var updatedClient = await fetchResponse.Content.ReadFromJsonAsync<ClientApiViewModel>();
            updatedClient!.FirstName.Should().Be(updatedFirstName);
            updatedClient.LastName.Should().Be(updatedLastName);
        }

        /// <summary>
        /// Testa o endpoint PUT quando o ID no URL não corresponde ao ID no corpo.
        /// Verifica se retorna status 400 Bad Request.
        /// </summary>
        [Fact]
        public async Task UpdateClient_WhenIDMismatch_ReturnBadRequest()
        {
            // Arrange
            var validClient = new ClientApiViewModel()
            {
                Id = Guid.NewGuid(), // Body ID
                UserId = Guid.NewGuid(),
                FirstName = "Test",
                LastName = "User",
                Address = "Test Address",
                PostalCode = "3000-003",
                PhoneNumber = "914444444",
                NIF = "555555555"
            };
            var pathId = Guid.NewGuid(); // Different URL ID

            // Act
            var response = await _httpClient.PutAsJsonAsync($"{API_BASE_PATH}/{pathId}", validClient);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // --- DELETE TESTS ---

        /// <summary>
        /// Testa o endpoint DELETE que remove um cliente existente.
        /// Verifica se retorna status 200 OK e o cliente é removido.
        /// </summary>
        [Fact]
        public async Task DeleteClient_WhenExists_ReturnOK()
        {
            // Arrange: Create a new client first
            var userId = Guid.NewGuid();
            var newUser = new UserApiViewModel()
            {
                Id = userId,
                Email = "deleteclient@mail.com",
                Password = "securePassword123!",
                UserType = "CLIENT",
                IsActive = true
            };

            var userResponse = await _httpClient.PostAsJsonAsync("/api/UserApi", newUser);
            userResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var clientIdToDelete = Guid.NewGuid();
            var clientToDelete = new ClientApiViewModel()
            {
                Id = clientIdToDelete,
                UserId = userId,
                FirstName = "ToDelete",
                LastName = "Client",
                Address = "789 Delete St",
                PostalCode = "4000-004",
                PhoneNumber = "915555555",
                NIF = "666666666"
            };

            var createResponse = await _httpClient.PostAsJsonAsync(API_BASE_PATH, clientToDelete);
            createResponse.EnsureSuccessStatusCode();

            // Act
            var response = await _httpClient.DeleteAsync($"{API_BASE_PATH}/{clientIdToDelete}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify deletion
            var checkResponse = await _httpClient.GetAsync($"{API_BASE_PATH}/{clientIdToDelete}");
            checkResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Testa o endpoint DELETE que tenta remover um cliente inexistente.
        /// Verifica se retorna status 404 Not Found.
        /// </summary>
        [Fact]
        public async Task DeleteClient_WhenDoesNotExist_ReturnNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _httpClient.DeleteAsync($"{API_BASE_PATH}/{nonExistentId}");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
        [Fact]
        public async Task GetAllClients_WhenClient_ReturnForbidden()
        {
            var clientHttp = _factory.CreateAuthenticatedClient(role: "CLIENT", userId: Guid.NewGuid());

            var response = await clientHttp.GetAsync(API_BASE_PATH);

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetClientById_WhenClientDoesNotOwnProfile_ReturnForbidden()
        {
            var allClients = await _httpClient.GetFromJsonAsync<IEnumerable<ClientApiViewModel>>(API_BASE_PATH);
            var clientId = allClients!.First(c => c.NIF == SEEDED_CLIENT_NIF).Id;
            var clientHttp = _factory.CreateAuthenticatedClient(role: "CLIENT", userId: Guid.NewGuid());

            var response = await clientHttp.GetAsync($"{API_BASE_PATH}/{clientId}");

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

    }
}