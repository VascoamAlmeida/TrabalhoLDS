using FluentAssertions;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models; // Required for Notification
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Services.Entities;
using Moq;

namespace LojaMaquinas.Business.Tests.Services
{
    /// <summary>
    /// Classe de testes unitários para o serviço de gestão de clientes.
    /// Testa as operações CRUD e validações de regras de negócio do ClientService.
    /// </summary>
    public class ClientServiceTests
    {
        /// <summary>
        /// Mock do repositório de clientes para simular operações na base de dados.
        /// </summary>
        private readonly Mock<IClientRepository> _clientRepositoryMock;

        /// <summary>
        /// Mock do serviço de notificações para verificar erros e validações.
        /// </summary>
        private readonly Mock<INotifierService> _notifierMock;

        /// <summary>
        /// Instância do serviço de clientes a ser testado.
        /// </summary>
        private readonly ClientService _clientService;

        /// <summary>
        /// Construtor que inicializa os mocks e a instância do serviço para cada teste.
        /// </summary>
        public ClientServiceTests()
        {
            _clientRepositoryMock = new Mock<IClientRepository>();
            _notifierMock = new Mock<INotifierService>();
            _clientService = new ClientService(_clientRepositoryMock.Object, _notifierMock.Object);
        }

        /// <summary>
        /// Testa a adição de um cliente válido.
        /// Verifica se o cliente é adicionado com sucesso e não gera notificações de erro.
        /// </summary>
        [Fact]
        public async Task Add_ValidClient_ShouldAddSuccessfully()
        {
            // Arrange
            var client = new Client
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Address = "123 Main St",
                PostalCode = "1234-567",
                PhoneNumber = "912345678",
                NIF = "123456789"
            };

            _clientRepositoryMock.Setup(x => x.GetByNif(client.NIF))
                .ReturnsAsync((Client)null);

            _clientRepositoryMock.Setup(x => x.Add(It.IsAny<Client>()))
                .Returns(Task.CompletedTask);

            // Act
            await _clientService.Add(client);

            // Assert
            _clientRepositoryMock.Verify(x => x.Add(It.Is<Client>(c =>
                c.NIF == client.NIF &&
                c.FirstName == client.FirstName &&
                c.LastName == client.LastName
            )), Times.Once);

            // FIXED: Verify Handle is NEVER called on success
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Never);
        }

        /// <summary>
        /// Testa a adição de um cliente com dados inválidos.
        /// Verifica se o serviço notifica os erros de validação e não adiciona o cliente.
        /// </summary>
        [Fact]
        public async Task Add_InvalidClient_ShouldNotifyValidationErrors()
        {
            // Arrange - Client with invalid data (empty FirstName)
            var client = new Client
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FirstName = "", // Invalid
                LastName = "Doe",
                Address = "123 Main St",
                PostalCode = "1234-567",
                PhoneNumber = "912345678",
                NIF = "123456789"
            };

            // Act
            await _clientService.Add(client);

            // Assert
            _clientRepositoryMock.Verify(x => x.Add(It.IsAny<Client>()), Times.Never);
            // FIXED: Verify Handle is called at least once for validation errors
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.AtLeastOnce);
        }

        /// <summary>
        /// Testa a adição de um cliente com NIF duplicado.
        /// Verifica se o serviço notifica o erro de negócio e não adiciona o cliente.
        /// </summary>
        [Fact]
        public async Task Add_DuplicateNIF_ShouldNotify()
        {
            // Arrange
            var existingClient = new Client
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Smith",
                Address = "456 Oak Ave",
                PostalCode = "1234-567",
                PhoneNumber = "913456789",
                NIF = "123456789"
            };

            var newClient = new Client
            {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Address = "789 Pine St",
                PostalCode = "1234-567",
                PhoneNumber = "914567890",
                NIF = "123456789" // Same NIF as existing client
            };

            _clientRepositoryMock.Setup(x => x.GetByNif(newClient.NIF))
                .ReturnsAsync(existingClient);

            // Act
            await _clientService.Add(newClient);

            // Assert
            _clientRepositoryMock.Verify(x => x.Add(It.IsAny<Client>()), Times.Never);
            // FIXED: Verify Handle is called once for the business rule failure
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Once);
        }

        /// <summary>
        /// Testa a atualização de um cliente válido.
        /// Verifica se o cliente é atualizado com sucesso e não gera notificações de erro.
        /// </summary>
        [Fact]
        public async Task Update_ValidClient_ShouldUpdateSuccessfully()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var client = new Client
            {
                Id = clientId,
                UserId = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Address = "123 Main St Updated",
                PostalCode = "1234-567",
                PhoneNumber = "912345678",
                NIF = "123456789"
            };

            // CRITICAL FIX: The service checks if existingClient.Id != client.Id.
            // For a successful update (same NIF, different fields), we must return the SAME client ID.
            _clientRepositoryMock.Setup(x => x.GetByNif(client.NIF))
                .ReturnsAsync(new Client { Id = clientId, NIF = client.NIF });

            _clientRepositoryMock.Setup(x => x.Update(It.IsAny<Client>()))
                .Returns(Task.CompletedTask);

            // Act
            await _clientService.Update(client);

            // Assert
            _clientRepositoryMock.Verify(x => x.Update(It.Is<Client>(c =>
                c.Id == client.Id &&
                c.Address == "123 Main St Updated"
            )), Times.Once);

            // FIXED: Verify Handle is NEVER called on success
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Never);
        }

        /// <summary>
        /// Testa a atualização de um cliente com NIF duplicado.
        /// Verifica se o serviço notifica o erro quando tenta atualizar para um NIF já existente.
        /// </summary>
        [Fact]
        public async Task Update_DuplicateNIF_ShouldNotify()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var existingClientId = Guid.NewGuid();

            var existingClient = new Client
            {
                Id = existingClientId, // Different ID
                UserId = Guid.NewGuid(),
                FirstName = "Jane",
                LastName = "Smith",
                Address = "456 Oak Ave",
                PostalCode = "1234-567",
                PhoneNumber = "913456789",
                NIF = "123456789"
            };

            var clientToUpdate = new Client
            {
                Id = clientId,
                UserId = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Address = "789 Pine St",
                PostalCode = "1234-567",
                PhoneNumber = "914567890",
                NIF = "123456789" // Same NIF as different client
            };

            // Setup the failing check: returns a client with a DIFFERENT ID but the same NIF
            _clientRepositoryMock.Setup(x => x.GetByNif(clientToUpdate.NIF))
                .ReturnsAsync(existingClient);

            // Act
            await _clientService.Update(clientToUpdate);

            // Assert
            _clientRepositoryMock.Verify(x => x.Update(It.IsAny<Client>()), Times.Never);
            // FIXED: Verify Handle is called once for the business rule failure
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Once);
        }

        /// <summary>
        /// Testa a remoção de um cliente inexistente.
        /// Verifica se o serviço notifica o erro quando o cliente não é encontrado.
        /// </summary>
        [Fact]
        public async Task Remove_ClientNotFound_ShouldNotify()
        {
            // Arrange
            var clientId = Guid.NewGuid();

            _clientRepositoryMock.Setup(x => x.GetClientWithOrders(clientId))
                .ReturnsAsync((Client)null);

            // Act
            await _clientService.Remove(clientId);

            // Assert
            _clientRepositoryMock.Verify(x => x.Remove(It.IsAny<Guid>()), Times.Never);
            // FIXED: Verify Handle is called once
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Once);
        }

        /// <summary>
        /// Testa a remoção de um cliente que tem pedidos associados.
        /// Verifica se o serviço notifica o erro de integridade referencial.
        /// </summary>
        [Fact]
        public async Task Remove_ClientWithOrders_ShouldNotify()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var client = new Client
            {
                Id = clientId,
                UserId = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Address = "123 Main St",
                PostalCode = "1234-567",
                PhoneNumber = "912345678",
                NIF = "123456789",
                Orders = new List<Order>
                {
                    new Order { Id = Guid.NewGuid(), ClientId = clientId }
                }
            };

            _clientRepositoryMock.Setup(x => x.GetClientWithOrders(clientId))
                .ReturnsAsync(client);

            // Act
            await _clientService.Remove(clientId);

            // Assert
            _clientRepositoryMock.Verify(x => x.Remove(It.IsAny<Guid>()), Times.Never);
            // FIXED: Verify Handle is called once
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Once);
        }

        /// <summary>
        /// Testa a remoção de um cliente sem pedidos associados.
        /// Verifica se o cliente é removido com sucesso e não gera notificações de erro.
        /// </summary>
        [Fact]
        public async Task Remove_ClientWithoutOrders_ShouldRemoveSuccessfully()
        {
            // Arrange
            var clientId = Guid.NewGuid();
            var client = new Client
            {
                Id = clientId,
                UserId = Guid.NewGuid(),
                FirstName = "John",
                LastName = "Doe",
                Address = "123 Main St",
                PostalCode = "1234-567",
                PhoneNumber = "912345678",
                NIF = "123456789",
                Orders = new List<Order>() // Empty orders list
            };

            _clientRepositoryMock.Setup(x => x.GetClientWithOrders(clientId))
                .ReturnsAsync(client);

            _clientRepositoryMock.Setup(x => x.Remove(clientId))
                .Returns(Task.CompletedTask);

            // Act
            await _clientService.Remove(clientId);

            // Assert
            _clientRepositoryMock.Verify(x => x.Remove(clientId), Times.Once);
            // FIXED: Verify Handle is NEVER called on success
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Never);
        }
    }
}