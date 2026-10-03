using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.Business.Interfaces.Entities
{
    /// <summary>
    /// Interface de serviço para gestão de pedidos (Orders).
    /// Contém a lógica de negócio e validações relacionadas com pedidos de compra e aluguel.
    /// </summary>
    public interface IOrderService : IDisposable
    {
        /// <summary>
        /// Adiciona um novo pedido ao sistema.
        /// Valida os dados, verifica disponibilidade de stock e atualiza estados.
        /// </summary>
        /// <param name="order">Pedido a ser adicionado</param>
        Task Add(Order order);

        /// <summary>
        /// Atualiza um pedido existente.
        /// Valida os dados e verifica se o pedido existe antes de atualizar.
        /// </summary>
        /// <param name="order">Pedido com os dados atualizados</param>
        Task Update(Order order);

        /// <summary>
        /// Remove um pedido do sistema.
        /// Remove também os registos associados (OrderRent se aplicável).
        /// </summary>
        /// <param name="id">Identificador único do pedido a remover</param>
        Task Remove(Guid id);
    }
}
