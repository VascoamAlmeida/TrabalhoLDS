using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Business.Validations.Entities;

namespace LojaMaquinas.Business.Services.Entities
{
    /// <summary>
    /// Implementação do serviço de gestão de pedidos (Orders).
    /// Contém lógica de negócio para validação e manipulação de pedidos de compra e aluguer.
    /// </summary>
    public class OrderService : BaseService, IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        /// <summary>
        /// Inicializa uma nova instância do serviço de pedidos.
        /// </summary>
        /// <param name="orderRepository">Repositório para acesso aos dados de pedidos</param>
        /// <param name="notifier">Serviço de notificações para comunicar erros</param>
        public OrderService(IOrderRepository orderRepository, INotifierService notifier) : base(notifier)
        {
            _orderRepository = orderRepository;
        }

        /// <summary>
        /// Adiciona um novo pedido ao sistema.
        /// Valida os dados, verifica se contém um item de stock e regista a data atual.
        /// </summary>
        /// <param name="order">Pedido a ser adicionado</param>
        public async Task Add(Order order)
        {
            if (!RunValidation(new OrderValidation(), order)) return;

            if(order.StockId == Guid.Empty)
            {
                NotifyWithMessage("The order must contain one item!");
                return;
            }

            order.OrderDate = DateTime.Now;

            await _orderRepository.Add(order);
        }

        /// <summary>
        /// Atualiza os dados de um pedido existente.
        /// Valida os dados e verifica se o pedido contém um item de stock.
        /// </summary>
        /// <param name="order">Pedido com os dados atualizados</param>
        public async Task Update(Order order)
        {
            if (!RunValidation(new OrderValidation(), order)) return;

            if (order.StockId == Guid.Empty )
            {
                NotifyWithMessage("The order must contain one item!");
                return;
            }

            await _orderRepository.Update(order);
        }

        /// <summary>
        /// Remove um pedido do sistema.
        /// Apenas permite remoção se o pedido estiver com estado Cancelled.
        /// </summary>
        /// <param name="id">Identificador único do pedido a remover</param>
        public async Task Remove(Guid id)
        {
            var existingOrder = await _orderRepository.GetById(id);
            if (existingOrder == null)
            {
                NotifyWithMessage("The order was not found!");
                return;
            }

            if (existingOrder.Status != OrderStatus.Cancelled)
            {
                NotifyWithMessage("The order can not be deleted!");
                return;
            }

            await _orderRepository.Remove(id);
        }

        /// <summary>
        /// Liberta os recursos utilizados pelo serviço.
        /// </summary>
        public void Dispose()
        {
            _orderRepository.Dispose();
        }
    }
}
