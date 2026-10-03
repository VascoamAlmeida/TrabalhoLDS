using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Business.Services.Entities;
using Moq;

namespace Tests.Services
{
    /// <summary>
    /// Classe de testes unitários para o serviço de gestão de pedidos.
    /// Testa as operações CRUD e validações de regras de negócio do OrderService.
    /// </summary>
    public class OrderServiceTests
    {
        /// <summary>
        /// Instância do serviço de pedidos a ser testado (System Under Test).
        /// </summary>
        private readonly OrderService _sut;

        /// <summary>
        /// Mock do repositório de pedidos para simular operações na base de dados.
        /// </summary>
        private readonly Mock<IOrderRepository> _orderRepositoryMock = new Mock<IOrderRepository>();

        /// <summary>
        /// Mock do serviço de notificações para verificar erros e validações.
        /// </summary>
        private readonly Mock<INotifierService> _notifierMock = new Mock<INotifierService>();

        /// <summary>
        /// Construtor que inicializa os mocks e a instância do serviço para cada teste.
        /// </summary>
        public OrderServiceTests()
        {
            _sut = new OrderService(_orderRepositoryMock.Object, _notifierMock.Object);
        }

        /// <summary>
        /// Cria uma instância de pedido válido para uso nos testes.
        /// </summary>
        /// <returns>Um pedido válido com dados preenchidos</returns>
        private static Order NewValidOrder() => new Order
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            StockId = Guid.NewGuid(),
            OrderDate = DateTime.Now,
            Total = 100,
            Type = OrderType.Sale,
            Status = OrderStatus.Active
        };


        /// <summary>
        /// Testa a adição de um pedido válido.
        /// Verifica se o pedido é adicionado com sucesso e não gera notificações de erro.
        /// </summary>
        [Fact]
        public async Task AddOrderServiceTest_ReturnOK()

        {
            // Arrange
            // Prepara para realizar o teste
            var validOrder = NewValidOrder();

            // Chamar o add no repositoria e returnar tarefa completa
            _orderRepositoryMock
                .Setup(r => r.Add(validOrder))
                .Returns(Task.CompletedTask);

            // Act
            // O que o teste vai realizar para ser testado, tem a resposta atual
            await _sut.Add(validOrder);

            // Assert
            // Verifica o que é expectavel
            // Repositorio é chamado um unica vez (Times.Once)
            _orderRepositoryMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Once);

            // Suposto nao emitir notificação de erro
            _notifierMock.Verify(n => n.Handle(It.IsAny<Notification>()), Times.Never);
        }

        /// <summary>
        /// Testa a adição de um pedido com StockId inválido.
        /// Verifica se o serviço notifica o erro de validação e não adiciona o pedido.
        /// </summary>
        [Fact]
        public async Task AddOrderServiceTest_ReturnNotOK()
        {
            // Arrange
            var validOrder = NewValidOrder();
            validOrder.StockId = Guid.Empty;

            // Chamar o notifier pois tem de apresentar uma
            _notifierMock.Setup(n => n.Handle(It.IsAny<Notification>()));

            // Act
            await _sut.Add(validOrder);

            // Assert
            // Repositorio é suposto não ir ao repositorio (Times.Nver), pois tem erro
            _orderRepositoryMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);

            // Suposto emitir notificação de erro, pois StockId esta errado
            _notifierMock.Verify(n => n.Handle(It.IsAny<Notification>()), Times.AtLeastOnce());
        }

        /// <summary>
        /// Testa a adição de um pedido mantendo o status Pending.
        /// Verifica se o status do pedido permanece inalterado após a adição.
        /// </summary>
        [Fact]
        public async Task AddOrderServiceTest_ReturnOK_WithStatusValid()
        {
            // Arrange
            var validOrder = NewValidOrder();
            validOrder.Status = OrderStatus.Pending;


            // Act
            await _sut.Add(validOrder);

            // Assert
            Assert.Equal(validOrder.Status, OrderStatus.Pending);
        }

        /// <summary>
        /// Testa a atualização de um pedido válido.
        /// Verifica se o pedido é atualizado com sucesso e não gera notificações de erro.
        /// </summary>
        [Fact]
        public async Task UpdateOrderServiceTest_ReturnOK()
        {
            // Arrange
            var validOrder = NewValidOrder();

            // Chamar o update no repositorio e retornar a tarefa completa
            _orderRepositoryMock
                .Setup(r => r.Update(validOrder))
                .Returns(Task.CompletedTask);

            // Act
            await _sut.Update(validOrder);

            // Assert
            // Repositorio é chamado um unica vez (Times.Once)
            _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Once);

            // // Suposto nao emitir notificação de erro
            _notifierMock.Verify(n => n.Handle(It.IsAny<Notification>()), Times.Never);
        }

        /// <summary>
        /// Testa a atualização de um pedido com StockId inválido.
        /// Verifica se o serviço notifica o erro de validação e não atualiza o pedido.
        /// </summary>
        [Fact]
        public async Task UpdateOrderServiceTest_ReturnNotOK_InvalidId()
        {
            // Arrange
            var validOrder = NewValidOrder();
            validOrder.StockId = Guid.Empty;

            // Chamar o notifier pois tem de apresentar uma
            _notifierMock.Setup(n => n.Handle(It.IsAny<Notification>()));


            // Act
            await _sut.Update(validOrder);

            // Assert
            // Repositorio é suposto não ir ao repositorio (Times.Nver), pois tem erro
            _orderRepositoryMock.Verify(r => r.Update(It.IsAny<Order>()), Times.Never);

            // Suposto emitir notificação de erro, pois StockId esta errado
            _notifierMock.Verify(n => n.Handle(It.IsAny<Notification>()), Times.AtLeastOnce);
        }


        /// <summary>
        /// Testa a tentativa de remoção de um pedido com status diferente de Cancelled.
        /// Verifica se o serviço notifica o erro e não permite a remoção.
        /// </summary>
        [Fact]
        public async Task RemoveOrderServiceTest_ReturnOK_WhenOrderStatusIsNotCancelled()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var validOrder = NewValidOrder();
            validOrder.Status = OrderStatus.Active;

            // Chamar o add no repositoria e returnar tarefa completa
            _orderRepositoryMock
                .Setup(r => r.GetById(orderId))
                .ReturnsAsync((validOrder));

            // Chamar o notifier pois tem de apresentar uma
            _notifierMock.Setup(n => n.Handle(It.IsAny<Notification>()));

            // Act
            await _sut.Remove(orderId);

            // Assert
            _orderRepositoryMock.Verify(r => r.Remove(It.IsAny<Guid>()), Times.Never);

            // // Suposto nao emitir notificação de erro
            _notifierMock.Verify(n => n.Handle(It.IsAny<Notification>()), Times.AtLeastOnce);
        }

        /// <summary>
        /// Testa a remoção de um pedido com status Cancelled.
        /// Verifica se o pedido é removido com sucesso e não gera notificações de erro.
        /// </summary>
        [Fact]
        public async Task RemoveOrderServiceTest_ReturnOK_WhenOrderStatusIsCancelled()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var validOrder = NewValidOrder();
            validOrder.Status = OrderStatus.Cancelled;

            // Chamar o add no repositoria e returnar tarefa completa
            _orderRepositoryMock
                .Setup(r => r.GetById(orderId))
                .ReturnsAsync((validOrder));

            // Chamar o notifier pois tem de apresentar uma
            _notifierMock.Setup(n => n.Handle(It.IsAny<Notification>()));

            // Act
            await _sut.Remove(orderId);

            // Assert
            _orderRepositoryMock.Verify(r => r.Remove(It.IsAny<Guid>()), Times.Once);

            // Suposto nao emitir notificação de erro
            _notifierMock.Verify(n => n.Handle(It.IsAny<Notification>()), Times.Never);
        }


        /// <summary>
        /// Testa a tentativa de remoção de um pedido inexistente.
        /// Verifica se o serviço notifica o erro quando o pedido não é encontrado.
        /// </summary>
        [Fact]
        public async Task RemoveOrderServiceTest_WhenOrderDoesNotExist()
        {
            // Arrange
            var orderId = Guid.NewGuid();


            _orderRepositoryMock.Setup(r => r.GetById(orderId))
                .ReturnsAsync((Order)null);

            _notifierMock.Setup(n => n.Handle(It.IsAny<Notification>()));

            // Act
            await _sut.Remove(orderId);

            // Assert
            // Repositorio é suposto não ir ao repositorio (Times.Nver), pois tem erro
            _orderRepositoryMock.Verify(r => r.Remove(It.IsAny<Guid>()), Times.Never);

            // Suposto emitir notificação de erro, pois ClientId esta errado
            _notifierMock.Verify(n => n.Handle(It.IsAny<Notification>()), Times.AtLeastOnce);
        }

        /// <summary>
        /// Testa a adição de um pedido com total igual a zero.
        /// Verifica se o serviço notifica o erro de validação e não adiciona o pedido.
        /// </summary>
        [Fact]
        public async Task AddOrderServiceTest_ReturnNotOK_TotalInvalid()
        {
            // Arrange
            var order = NewValidOrder();
            order.Total = 0;
            _notifierMock.Setup(n => n.Handle(It.IsAny<Notification>()));

            // Act
            await _sut.Add(order);

            // Assert
            _orderRepositoryMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);
            _notifierMock.Verify(n => n.Handle(It.IsAny<Notification>()), Times.AtLeastOnce);
        }

        /// <summary>
        /// Testa a adição de um pedido com total negativo.
        /// Verifica se o serviço notifica o erro de validação e não adiciona o pedido.
        /// </summary>
        [Fact]
        public async Task AddOrderServiceTest_ReturnNotOK_TotalNegative()
        {
            // Arrange
            var order = NewValidOrder();
            order.Total = -1;
            _notifierMock.Setup(n => n.Handle(It.IsAny<Notification>()));

            // Act
            await _sut.Add(order);

            // Assert
            _orderRepositoryMock.Verify(r => r.Add(It.IsAny<Order>()), Times.Never);
            _notifierMock.Verify(n => n.Handle(It.IsAny<Notification>()), Times.AtLeastOnce);
        }
    }
}
