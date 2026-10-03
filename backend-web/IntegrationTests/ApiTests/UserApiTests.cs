using FluentAssertions;
using LojaMaquinas.Data.Contexts;
using LojaMaquinas.WebApp.ViewModels.API;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace IntegrationTests.ApiTests
{
    /// <summary>
    /// Classe de testes de integração para a API de Utilizadores (UserApi).
    /// Testa as operações de criação, leitura, atualização, desativação e exclusão de utilizadores.
    /// </summary>
    public class UserApiTests : IClassFixture<IntegrationTestWebAppFactory>
    {
        private readonly HttpClient _httpClient;
        private readonly IntegrationTestWebAppFactory _factory;
        private const string SEEDED_USER_EMAIL = "admin@mail.com";
        private const string API_BASE_PATH = "/api/UserApi";

        /// <summary>
        /// Construtor responsável por inicializar o cliente HTTP e repor a base de dados antes de cada teste.
        /// </summary>
        /// <param name="factory">Instância da fábrica da aplicação de teste.</param>
        public UserApiTests(IntegrationTestWebAppFactory factory)
        {
            _factory = factory;
            _httpClient = _factory.CreateAuthenticatedClient();

            // Reset the database before each test
            ResetDatabase();
        }

        /// <summary>
        /// Restaura a base de dados de teste para o estado inicial.
        /// </summary>
        private void ResetDatabase()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            Seeding.InitializeTestDB(db);
        }

        // --- GET TESTS ---
        /// <summary>
        /// Testa a obtenção de todos os utilizadores.
        /// Deve retornar status HTTP 200 OK e incluir o utilizador seed.
        /// </summary>
        [Fact]
        public async Task GetAllUsers_ReturnOK()
        {
            // Act
            var response = await _httpClient.GetAsync(API_BASE_PATH);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserApiViewModel>>();
            users.Should().NotBeNullOrEmpty();
            users.Should().Contain(u => u.Email == SEEDED_USER_EMAIL);
        }

        // --- POST (CREATE) TESTS ---
        /// <summary>
        /// Testa a criação de um novo utilizador válido.
        /// Deve retornar status HTTP 200 OK.
        /// </summary>
        [Fact]
        public async Task CreateUser_WhenValid_ReturnOK()
        {
            // Arrange
            var newUser = new UserApiViewModel()
            {
                Id = Guid.NewGuid(),
                Email = "newuser@gmail.com",
                Password = "securePassword123!",
                UserType = "CLIENT",
                IsActive = true
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(API_BASE_PATH, newUser);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // creation by fetching the new user
            var fetchResponse = await _httpClient.GetAsync($"{API_BASE_PATH}/email/{newUser.Email}");
            fetchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Testa a tentativa de criar um utilizador com um email duplicado.
        /// Deve retornar status HTTP 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task CreateUser_WhenDuplicateEmail_ReturnBadRequest()
        {
            // Arrange
            var duplicateUser = new UserApiViewModel()
            {
                Email = SEEDED_USER_EMAIL,
                Password = "anyPassword",
                UserType = "client",
                IsActive = true
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(API_BASE_PATH, duplicateUser);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Testa a criação de um utilizador com dados inválidos (email e password incorretos).
        /// Deve retornar status HTTP 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task CreateUser_WhenValidationFails_ReturnBadRequest()
        {
            // Arrange
            var invalidUser = new UserApiViewModel()
            {
                Email = "invalid", // Invalid format
                Password = "short", // Likely too short
                UserType = "client",
                IsActive = true
            };

            // Act
            var response = await _httpClient.PostAsJsonAsync(API_BASE_PATH, invalidUser);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // --- PUT (UPDATE) TESTS ---
        /// <summary>
        /// Testa a atualização de um utilizador existente com dados válidos.
        /// Deve retornar status HTTP 200 OK.
        /// </summary>
        [Fact]
        public async Task UpdateUser_WhenValid_ReturnOK()
        {
            // Arrange
            var allUsers = await _httpClient.GetFromJsonAsync<IEnumerable<UserApiViewModel>>(API_BASE_PATH);
            var userToUpdate = allUsers!.First(u => u.Email == SEEDED_USER_EMAIL);

            var updatedEmail = "admin.updated@gmail.com";
            userToUpdate.Email = updatedEmail;

            // Act
            var response = await _httpClient.PutAsJsonAsync($"{API_BASE_PATH}/{userToUpdate.Id}", userToUpdate);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify the update
            var fetchResponse = await _httpClient.GetAsync($"{API_BASE_PATH}/email/{updatedEmail}");
            fetchResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        /// <summary>
        /// Testa a tentativa de atualização quando o ID do caminho não corresponde ao do corpo.
        /// Deve retornar status HTTP 400 BadRequest.
        /// </summary>
        [Fact]
        public async Task UpdateUser_WhenIDMismatch_ReturnBadRequest()
        {
            // Arrange
            var validUser = new UserApiViewModel()
            {
                Id = Guid.NewGuid(), // A different ID
                Email = "test@test.com",
                Password = "securePassword123",
                UserType = "client",
                IsActive = true
            };
            var pathId = Guid.NewGuid();

            // Act
            var response = await _httpClient.PutAsJsonAsync($"{API_BASE_PATH}/{pathId}", validUser);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // --- PATCH (DEACTIVATE) TESTS ---
        /// <summary>
        /// Testa a desativação de um utilizador existente.
        /// Deve retornar status HTTP 200 OK e alterar o campo <see cref="UserApiViewModel.IsActive"/> para false.
        /// </summary>
        [Fact]
        public async Task DeactivateUser_WhenExists_ReturnOK()
        {
            // Arrange
            var allUsers = await _httpClient.GetFromJsonAsync<IEnumerable<UserApiViewModel>>(API_BASE_PATH);
            var userIdToDeactivate = allUsers!.First(u => u.Email == SEEDED_USER_EMAIL).Id;

            // Act
            var response = await _httpClient.PatchAsync($"{API_BASE_PATH}/{userIdToDeactivate}/deactivate", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Verify status change
            var fetchResponse = await _httpClient.GetFromJsonAsync<UserApiViewModel>($"{API_BASE_PATH}/{userIdToDeactivate}");
            fetchResponse.Should().NotBeNull();
            fetchResponse!.IsActive.Should().BeFalse();
        }

        /// <summary>
        /// Testa a tentativa de desativar um utilizador inexistente.
        /// Deve retornar status HTTP 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeactivateUser_WhenDoesNotExist_ReturnNotFound()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var response = await _httpClient.PatchAsync($"{API_BASE_PATH}/{nonExistentId}/deactivate", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // --- DELETE TESTS ---

        /// <summary>
        /// Testa a exclusão de um utilizador existente.
        /// Deve retornar status HTTP 200 OK e o utilizador deve ser removido.
        /// </summary>
        [Fact]
        public async Task DeleteUser_WhenExists_ReturnOK()
        {
            // Arrange
            var userIdToDelete = Guid.NewGuid();
            var userToDelete = new UserApiViewModel()
            {
                Id = userIdToDelete,
                Email = "todelete@gmail.com",
                Password = "safePassword123!",
                UserType = "CLIENT",
                IsActive = true
            };

            // 1. Create the user
            var createResponse = await _httpClient.PostAsJsonAsync(API_BASE_PATH, userToDelete);
            createResponse.EnsureSuccessStatusCode();

            // Act
            var response = await _httpClient.DeleteAsync($"{API_BASE_PATH}/{userIdToDelete}");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            // Verify deletion
            var checkResponse = await _httpClient.GetAsync($"{API_BASE_PATH}/{userIdToDelete}");
            checkResponse.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Testa a tentativa de exclusão de um utilizador inexistente.
        /// Deve retornar status HTTP 404 NotFound.
        /// </summary>
        [Fact]
        public async Task DeleteUser_WhenDoesNotExist_ReturnNotFound()
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