using FluentAssertions;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Business.Services.Entities;
using LojaMaquinas.Data.Repositories.Parameters;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace Tests.Services
{
    /// <summary>
    /// Classe de testes unitários para o serviço de gestão de pedidos de aluguer.
    /// Testa as operações de adição, atualização, remoção e transições de estado dos OrderRents.
    /// </summary>
    public class OrderRentServiceTests
    {
        /// <summary>
        /// Instância do serviço de pedidos de aluguer a ser testado.
        /// </summary>
        private readonly OrderRentService _orderRentService;

        /// <summary>
        /// Mock do repositório de pedidos de aluguer para simular operações na base de dados.
        /// </summary>
        private readonly Mock<IOrderRentRepository> _orderRentRepositoryMock = new Mock<IOrderRentRepository>();

        /// <summary>
        /// Mock do serviço de notificações para verificar erros e validações.
        /// </summary>
        private readonly Mock<INotifierService> _notifierMock = new Mock<INotifierService>();

        /// <summary>
        /// Construtor que inicializa os mocks e a instância do serviço para cada teste.
        /// </summary>
        public OrderRentServiceTests()
        {
            _orderRentService = new OrderRentService(_notifierMock.Object, _orderRentRepositoryMock.Object);
        }

        #region Add Tests
        /// <summary>
        /// Testa a adição de um pedido de aluguer com dados válidos.
        /// Verifica se o pedido é criado com status "Approved" e não gera notificações de erro.
        /// </summary>
        [Fact]
        public async Task Add_OrderRent_WhenDataIsValid()
        {

            //Arrange
            var orderRent = new OrderRent
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                InitialDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                CompanyImage = "company.jpg",
            };

            _orderRentRepositoryMock
                .Setup(r => r.Find(It.IsAny<Expression<Func<OrderRent, bool>>>()))
                .ReturnsAsync(new List<OrderRent>());

            _orderRentRepositoryMock
                .Setup(r => r.Add(It.IsAny<OrderRent>()))
                .Returns(Task.CompletedTask);

            //Act
            await _orderRentService.Add(orderRent);

            //Assert
            orderRent.Status.Should().Be(LojaMaquinas.Business.Models.Enums.OrderRentStatus.Approved);
            _orderRentRepositoryMock.Verify(r => r.Add(It.IsAny<OrderRent>()), Times.Once);

            // Suposto nao emitir notificação de erro
            _notifierMock.Verify(n => n.Handle(It.IsAny<Notification>()), Times.Never);

        }

        /// <summary>
        /// Testa a tentativa de adicionar um pedido de aluguer que já existe.
        /// Verifica se o serviço não adiciona o pedido duplicado.
        /// </summary>
        [Fact]
        public async Task Add_NOT_WhenOrderRentAlreadyExists()
        {
            // Arrange
            var orderRentId = Guid.NewGuid();
            var orderRent = new OrderRent
            {
                Id = orderRentId,
                OrderId = Guid.NewGuid(),
                InitialDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                CompanyImage = "company.jpg"
            };

            var existingOrderRents = new List<OrderRent>
            {
                new OrderRent { Id = orderRentId }
            };

            _orderRentRepositoryMock
                .Setup(r => r.Find(It.IsAny<Expression<Func<OrderRent, bool>>>()))
                .ReturnsAsync(existingOrderRents);

            // Act
            await _orderRentService.Add(orderRent);

            // Assert
            _orderRentRepositoryMock.Verify(r => r.Add(It.IsAny<OrderRent>()), Times.Never);
        }

        /// <summary>
        /// Testa a alteração automática do status para "Approved" ao adicionar um pedido de aluguer.
        /// Verifica se o status é alterado independentemente do status inicial fornecido.
        /// </summary>
        [Fact]
        public async Task Add_StatusChangeApproved_WhenAdd()
        {
            // Arrange
            var orderRent = new OrderRent
            {
                Id = Guid.NewGuid(),
                OrderId = Guid.NewGuid(),
                Status = OrderRentStatus.Active,
                InitialDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                CompanyImage = "company.jpg"
            };

            _orderRentRepositoryMock
                .Setup(r => r.Find(It.IsAny<Expression<Func<OrderRent, bool>>>()))
                .ReturnsAsync(new List<OrderRent>());

            // Act
            await _orderRentService.Add(orderRent);

            // Assert
            orderRent.Status.Should().Be(OrderRentStatus.Approved);
        }
        #endregion

        #region Remove Tests
        /// <summary>
        /// Testa a remoção de um pedido de aluguer com status "Cancelled".
        /// Verifica se o pedido é removido com sucesso quando o status é Cancelled.
        /// </summary>
        [Fact]
        public async Task Remove_OrderRent_IfStatusCancelled()
        {
            var orderRentId = Guid.NewGuid();
            var orderRent = new OrderRent
            {
                Id = orderRentId,
                OrderId = Guid.NewGuid(),
                Status = OrderRentStatus.Cancelled,
                InitialDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                CompanyImage = "company.jpg"
            };

            _orderRentRepositoryMock
                .Setup(r => r.GetOrderRentAndOrderAndContractByOrderRentId(orderRentId))
                .ReturnsAsync(orderRent);

            _orderRentRepositoryMock
                .Setup(r => r.Remove(orderRentId))
                .Returns(Task.CompletedTask);

            await _orderRentService.Remove(orderRentId);

            _orderRentRepositoryMock.Verify(r => r.Remove(orderRentId), Times.Once);
        }

        /// <summary>
        /// Testa a tentativa de remoção de um pedido de aluguer com status diferente de "Cancelled".
        /// Verifica se o serviço não permite a remoção quando o status não é Cancelled.
        /// </summary>
        [Fact]
        public async Task Remove_MustNot_WhenStatusNotCancelled()
        {
            // Arrange
            var orderRentId = Guid.NewGuid();
            var orderRent = new OrderRent
            {
                Id = orderRentId,
                OrderId = Guid.NewGuid(),
                Status = OrderRentStatus.Approved,
                InitialDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                CompanyImage = "company.jpg"
            };

            _orderRentRepositoryMock
                .Setup(r => r.GetById(orderRentId))
                .ReturnsAsync(orderRent);

            // Act
            await _orderRentService.Remove(orderRentId);

            // Assert
            _orderRentRepositoryMock.Verify(r => r.Remove(It.IsAny<Guid>()), Times.Never);
        }
        #endregion

        #region Update Tests
        /// <summary>
        /// Testa a tentativa de atualização de um pedido de aluguer com status "Completed".
        /// Verifica se o serviço não permite atualizar pedidos já completos.
        /// </summary>
        [Fact]
        public async Task Update_MustNot_WhenStatusCompleted()
        {
            // Arrange
            var orderRentId = Guid.NewGuid();
            var currentOrderRent = new OrderRent
            {
                Id = orderRentId,
                OrderId = Guid.NewGuid(),
                Status = OrderRentStatus.Completed,
                InitialDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                CompanyImage = "company.jpg",
                UserImage = "user.jpg"
            };

            var updateOrderRent = new OrderRent
            {
                Id = orderRentId,
                OrderId = currentOrderRent.OrderId,
                Status = OrderRentStatus.Approved,
                InitialDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(7),
                CompanyImage = "company.jpg"
            };

            _orderRentRepositoryMock
                .Setup(r => r.GetById(orderRentId))
                .ReturnsAsync(currentOrderRent);

            // Act
            await _orderRentService.Update(updateOrderRent);

            // Assert
            _orderRentRepositoryMock.Verify(r => r.Update(It.IsAny<OrderRent>()), Times.Never);
        }

        /// <summary>
        /// Testa a transição de status para "Completed" quando o pedido está ativo,
        /// a data final já passou e é fornecida uma imagem do utilizador.
        /// Verifica se o status muda para Completed e a imagem do utilizador é atualizada.
        /// </summary>
        [Fact]
        public async Task Update_StatusChangeToCompleted_WhenActiveAndAfterEndDateAndWithUserImage()
        {
            // Arrange
            var orderRentId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            var currentOrderRent = new OrderRent
            {
                Id = orderRentId,
                OrderId = orderId,
                Status = OrderRentStatus.Active,
                InitialDate = DateTime.Now.AddDays(-7),
                EndDate = DateTime.Now.AddDays(-1), // Já passou
                CompanyImage = "company.jpg"
            };

            var updateOrderRent = new OrderRent
            {
                Id = orderRentId,
                OrderId = orderId,
                Status = OrderRentStatus.Completed,
                UserImage = "user.jpg",
                InitialDate = currentOrderRent.InitialDate,
                EndDate = currentOrderRent.EndDate,
                CompanyImage = "company.jpg"
            };

            _orderRentRepositoryMock
                .Setup(r => r.GetById(orderRentId))
                .ReturnsAsync(currentOrderRent);

            _orderRentRepositoryMock
                .Setup(r => r.Update(It.IsAny<OrderRent>()))
                .Returns(Task.CompletedTask);

            // Act
            await _orderRentService.Update(updateOrderRent);

            // Assert
            currentOrderRent.Status.Should().Be(OrderRentStatus.Completed);
            currentOrderRent.UserImage.Should().Be("user.jpg");
            _orderRentRepositoryMock.Verify(r => r.Update(currentOrderRent), Times.Once);
        }

        /// <summary>
        /// Testa a atualização para status "Completed" quando o pedido está ativo e é fornecida uma imagem do utilizador.
        /// Verifica se o status é alterado para Completed e a imagem do utilizador é guardada corretamente.
        /// </summary>
        [Fact]
        public async Task Update_MustChangeForCompleted_WhenActiveEUserImageGiven()
        {
            // Arrange
            var orderRentId = Guid.NewGuid();
            var orderId = Guid.NewGuid();

            var currentOrderRent = new OrderRent
            {
                Id = orderRentId,
                OrderId = orderId,
                Status = OrderRentStatus.Active,
                InitialDate = DateTime.Now.AddDays(-7),
                EndDate = DateTime.Now.AddDays(-1),
                CompanyImage = "company.jpg"
            };

            var updateOrderRent = new OrderRent
            {
                Id = orderRentId,
                OrderId = orderId,
                Status = OrderRentStatus.Completed,
                UserImage = "user-image.jpg",
                InitialDate = currentOrderRent.InitialDate,
                EndDate = currentOrderRent.EndDate,
                CompanyImage = "company.jpg"
            };

            _orderRentRepositoryMock
                .Setup(r => r.GetById(orderRentId))
                .ReturnsAsync(currentOrderRent);

            _orderRentRepositoryMock
                .Setup(r => r.Update(It.IsAny<OrderRent>()))
                .Returns(Task.CompletedTask);

            // Act
            await _orderRentService.Update(updateOrderRent);

            // Assert
            currentOrderRent.Status.Should().Be(OrderRentStatus.Completed);
            currentOrderRent.UserImage.Should().Be("user-image.jpg");
            _orderRentRepositoryMock.Verify(r => r.Update(currentOrderRent), Times.Once);
        }

        #endregion
    }
}