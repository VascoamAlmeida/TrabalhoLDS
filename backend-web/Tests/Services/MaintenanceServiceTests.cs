using FluentAssertions;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models; // ADDED: Required for Notification
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Business.Services.Entities;
using Moq;

namespace LojaMaquinas.Business.Tests.Services
{
    /// <summary>
    /// Classe de testes unitários para o serviço de gestão de manutenções.
    /// Testa as operações CRUD e validações de regras de negócio do MaintenanceService.
    /// </summary>
    public class MaintenanceServiceTests
    {
        /// <summary>
        /// Mock do repositório de manutenções para simular operações na base de dados.
        /// </summary>
        private readonly Mock<IMaintenanceRepository> _maintenanceRepositoryMock;

        /// <summary>
        /// Mock do repositório de pedidos para validar a existência de pedidos associados.
        /// </summary>
        private readonly Mock<IOrderRepository> _orderRepositoryMock;

        /// <summary>
        /// Mock do serviço de notificações para verificar erros e validações.
        /// </summary>
        private readonly Mock<INotifierService> _notifierMock;

        /// <summary>
        /// Instância do serviço de manutenções a ser testado.
        /// </summary>
        private readonly MaintenanceService _maintenanceService;

        /// <summary>
        /// Construtor que inicializa os mocks e a instância do serviço para cada teste.
        /// </summary>
        public MaintenanceServiceTests()
        {
            _maintenanceRepositoryMock = new Mock<IMaintenanceRepository>();
            _orderRepositoryMock = new Mock<IOrderRepository>();
            _notifierMock = new Mock<INotifierService>();
            _maintenanceService = new MaintenanceService(
                _maintenanceRepositoryMock.Object,
                _orderRepositoryMock.Object,
                _notifierMock.Object
            );
        }

        /// <summary>
        /// Testa a adição de uma manutenção válida.
        /// Verifica se a manutenção é adicionada com sucesso e não gera notificações de erro.
        /// </summary>
        [Fact]
        public async Task Add_ValidMaintenance_ShouldAddSuccessfully()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                ClientId = Guid.NewGuid(),
                OrderDate = DateTime.Now,
                Status = OrderStatus.Completed
            };

            var maintenance = new Maintenance
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ScheduledDate = DateTime.Now.AddDays(7),
                Price = 150.00m,
                Obs = "Regular maintenance",
                Status = MaintenanceStatus.Pending
            };

            _orderRepositoryMock.Setup(x => x.GetById(orderId))
                .ReturnsAsync(order);

            _maintenanceRepositoryMock.Setup(x => x.Add(It.IsAny<Maintenance>()))
                .Returns(Task.CompletedTask);

            // Act
            await _maintenanceService.Add(maintenance);

            // Assert
            _maintenanceRepositoryMock.Verify(x => x.Add(It.Is<Maintenance>(m =>
                m.OrderId == orderId &&
                m.Price == 150.00m &&
                m.Status == MaintenanceStatus.Pending
            )), Times.Once);

            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Never);
        }

        /// <summary>
        /// Testa a adição de uma manutenção com dados inválidos.
        /// Verifica se o serviço notifica os erros de validação e não adiciona a manutenção.
        /// </summary>
        [Fact]
        public async Task Add_InvalidMaintenance_ShouldNotifyValidationErrors()
        {
            // Arrange - Maintenance with invalid price
            var maintenance = new Maintenance
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                ScheduledDate = DateTime.Now.AddDays(7),
                Price = -50.00m, // Invalid
                Obs = "Regular maintenance",
                Status = MaintenanceStatus.Pending
            };

            // Act
            await _maintenanceService.Add(maintenance);

            // Assert
            _maintenanceRepositoryMock.Verify(x => x.Add(It.IsAny<Maintenance>()), Times.Never);
            
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.AtLeastOnce);
        }

        /// <summary>
        /// Testa a adição de uma manutenção associada a um pedido inexistente.
        /// Verifica se o serviço notifica o erro e não adiciona a manutenção.
        /// </summary>
        [Fact]
        public async Task Add_OrderNotFound_ShouldNotify()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var maintenance = new Maintenance
            {
                Id = Guid.NewGuid(),
                OrderId = orderId,
                ScheduledDate = DateTime.Now.AddDays(7),
                Price = 150.00m,
                Obs = "Regular maintenance",
                Status = MaintenanceStatus.Pending
            };

            _orderRepositoryMock.Setup(x => x.GetById(orderId))
                .ReturnsAsync((Order)null);

            // Act
            await _maintenanceService.Add(maintenance);

            // Assert
            _maintenanceRepositoryMock.Verify(x => x.Add(It.IsAny<Maintenance>()), Times.Never);
            
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Once);
        }

        /// <summary>
        /// Testa a atualização de uma manutenção válida.
        /// Verifica se a manutenção é atualizada com sucesso e não gera notificações de erro.
        /// </summary>
        [Fact]
        public async Task Update_ValidMaintenance_ShouldUpdateSuccessfully()
        {
            // Arrange
            var maintenance = new Maintenance
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                ScheduledDate = DateTime.Now.AddDays(10),
                Price = 200.00m,
                Obs = "Updated maintenance details",
                Status = MaintenanceStatus.InProgress
            };

            _maintenanceRepositoryMock.Setup(x => x.Update(It.IsAny<Maintenance>()))
                .Returns(Task.CompletedTask);

            // Act
            await _maintenanceService.Update(maintenance);

            // Assert
            _maintenanceRepositoryMock.Verify(x => x.Update(It.Is<Maintenance>(m =>
                m.Id == maintenance.Id &&
                m.Price == 200.00m &&
                m.Status == MaintenanceStatus.InProgress &&
                m.Obs == "Updated maintenance details"
            )), Times.Once);

            
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Never);
        }

        /// <summary>
        /// Testa a atualização de uma manutenção com dados inválidos.
        /// Verifica se o serviço notifica os erros de validação e não atualiza a manutenção.
        /// </summary>
        [Fact]
        public async Task Update_InvalidMaintenance_ShouldNotifyValidationErrors()
        {
            // Arrange - Maintenance with invalid data
            var maintenance = new Maintenance
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                ScheduledDate = DateTime.Now.AddDays(10),
                Price = -100.00m, // Invalid
                Obs = "Updated maintenance",
                Status = MaintenanceStatus.Completed
            };

            // Act
            await _maintenanceService.Update(maintenance);

            // Assert
            _maintenanceRepositoryMock.Verify(x => x.Update(It.IsAny<Maintenance>()), Times.Never);
            
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.AtLeastOnce);
        }

        /// <summary>
        /// Testa a remoção de uma manutenção inexistente.
        /// Verifica se o serviço notifica o erro quando a manutenção não é encontrada.
        /// </summary>
        [Fact]
        public async Task Remove_MaintenanceNotFound_ShouldNotify()
        {
            // Arrange
            var maintenanceId = Guid.NewGuid();

            _maintenanceRepositoryMock.Setup(x => x.GetById(maintenanceId))
                .ReturnsAsync((Maintenance)null);

            // Act
            await _maintenanceService.Remove(maintenanceId);

            // Assert
            _maintenanceRepositoryMock.Verify(x => x.Remove(It.IsAny<Guid>()), Times.Never);
            
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Once);
        }

        /// <summary>
        /// Testa a remoção de uma manutenção válida.
        /// Verifica se a manutenção é removida com sucesso e não gera notificações de erro.
        /// </summary>
        [Fact]
        public async Task Remove_ValidMaintenance_ShouldRemoveSuccessfully()
        {
            // Arrange
            var maintenanceId = Guid.NewGuid();
            var maintenance = new Maintenance
            {
                Id = maintenanceId,
                OrderId = Guid.NewGuid(),
                ScheduledDate = DateTime.Now.AddDays(7),
                Price = 150.00m,
                Obs = "Regular maintenance",
                Status = MaintenanceStatus.Pending
            };

            _maintenanceRepositoryMock.Setup(x => x.GetById(maintenanceId))
                .ReturnsAsync(maintenance);

            _maintenanceRepositoryMock.Setup(x => x.Remove(maintenanceId))
                .Returns(Task.CompletedTask);

            // Act
            await _maintenanceService.Remove(maintenanceId);

            // Assert
            _maintenanceRepositoryMock.Verify(x => x.Remove(maintenanceId), Times.Once);
            
            _notifierMock.Verify(x => x.Handle(It.IsAny<Notification>()), Times.Never);
        }
    }
}