using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.Business.Interfaces.Entities
{
    /// <summary>
    /// Interface de repositório para gestão de pedidos (Orders) na base de dados.
    /// Fornece operações específicas para consulta de pedidos e suas relações.
    /// </summary>
    public interface IOrderRepository : IRepository<Order>
    {
        /// <summary>
        /// Obtém todos os pedidos de um cliente específico.
        /// </summary>
        /// <param name="clientId">Identificador único do cliente</param>
        /// <returns>Coleção de pedidos do cliente</returns>
        Task<IEnumerable<Order>> GetOrdersByClientId(Guid clientId);

        /// <summary>
        /// Obtém todos os pedidos com os dados dos utilizadores associados.
        /// </summary>
        /// <returns>Coleção de pedidos com relações User carregadas</returns>
        Task<IEnumerable<Order>> GetOrdersAndUsers();

        /// <summary>
        /// Obtém um pedido com os dados do cliente e stock associados.
        /// </summary>
        /// <returns>Pedido com relações Client e Stock carregadas</returns>
        Task<IEnumerable<Order>> GetOrdersAndClientAndStock();

        /// <summary>
        /// Obtém um pedido com os dados do cliente e stock associados.
        /// </summary>
        /// <param name="id">Identificador único do pedido</param>
        /// <returns>Pedido com relações Client e Stock carregadas</returns>
        Task<Order> GetOrderAndClientAndStockByOrderId(Guid id);

        /// <summary>
        /// Obtém o aluguel (OrderRent) associado a um pedido.
        /// </summary>
        /// <param name="orderId">Identificador único do pedido</param>
        /// <returns>OrderRent associado ao pedido ou null se não for aluguel</returns>
        Task<OrderRent> GetOrderRentByOrderId(Guid orderId);

        /// <summary>
        /// Remove um aluguel (OrderRent) associado a um pedido.
        /// </summary>
        /// <param name="orderRent">Entidade OrderRent a ser removida</param>
        Task RemoveOrderRentByOrder(OrderRent orderRent);

        /// <summary>
        /// Obtém todos os pedidos de um cliente específico, com os detalhes do Stock
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        Task<IEnumerable<Order>> GetOrdersAndStockByClientId(Guid clientId);

        /// <summary>
        /// Obtém o valor total de receita de vendas (Sale) para um mês específico.
        /// </summary>
        /// <param name="year">Ano da consulta</param>
        /// <param name="month">Mês da consulta (1-12)</param>
        /// <returns>Valor total de receita das vendas do mês especificado</returns>
        Task<decimal> GetMonthlySalesRevenue(int year, int month);

        /// <summary>
        /// Obtém os dados de receita de vendas mensais para os últimos N meses.
        /// Útil para construir gráficos de evolução de vendas.
        /// </summary>
        /// <param name="numberOfMonths">Número de meses a consultar (incluindo o mês atual)</param>
        /// <returns>Dicionário com nome do mês (abreviado) e valor de receita correspondente</returns>
        Task<Dictionary<string, decimal>> GetMonthlyRevenueData(int numberOfMonths);

        /// <summary>
        /// Obtém o número de alugueres (Rent) num período específico.
        /// </summary>
        /// <param name="startDate">Data de início do período</param>
        /// <param name="endDate">Data de fim do período</param>
        /// <returns>Número total de alugueres no período especificado</returns>
        Task<int> GetWeeklyRentalsCount(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Obtém os dados de alugueres semanais para as últimas N semanas.
        /// Útil para construir gráficos de evolução de alugueres.
        /// </summary>
        /// <param name="numberOfWeeks">Número de semanas a consultar (incluindo a semana atual)</param>
        /// <returns>Lista com o número de alugueres de cada semana</returns>
        Task<List<int>> GetWeeklyRentalsData(int numberOfWeeks);
    }
}
