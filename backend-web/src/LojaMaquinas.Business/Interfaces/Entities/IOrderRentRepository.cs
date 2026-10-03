using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.Business.Interfaces.Entities
{
    /// <summary>
    /// Interface de repositório para gestão de aluguéis (OrderRent) na base de dados.
    /// Fornece operações específicas para consulta de aluguéis e suas relações.
    /// </summary>
    public interface IOrderRentRepository : IRepository<OrderRent>
    {
        /// <summary>
        /// Obtém um aluguel com o pedido (Order) associado.
        /// </summary>
        /// <param name="orderRentId">Identificador único do OrderRent</param>
        /// <returns>OrderRent com a relação Order carregada</returns>
        Task<OrderRent> GetOrderAndRentOrderByOrderId(Guid orderRentId);

        /// <summary>
        /// Obtém um aluguel com o contrato associado.
        /// </summary>
        /// <param name="orderRentId">Identificador único do OrderRent</param>
        /// <returns>OrderRent com a relação Contract carregada</returns>
        Task<OrderRent> GetOrderRentAndContractByOrderRentId(Guid orderRentId);

        /// <summary>
        /// Obtém um aluguel com o pedido e contrato associados.
        /// </summary>
        /// <param name="orderRentId">Identificador único do OrderRent</param>
        /// <returns>OrderRent com relações Order e Contract carregadas</returns>
        Task<OrderRent> GetOrderRentAndOrderAndContractByOrderRentId(Guid orderRentId);

        /// <summary>
        /// Remove um contrato associado a um aluguel.
        /// </summary>
        /// <param name="contract">Entidade Contract a ser removida</param>
        Task DeleteContractByOrderRent(Contract contract);

        /// <summary>
        /// Obtém todos os alugueres cujo contrato termina entre duas datas.
        /// Usado para o alerta de fim de contrato.
        /// </summary>
        /// <param name="start">Data inicial (incluída)</param>
        /// <param name="end">Data final (incluída)</param>
        /// <returns>Lista de alugueres a terminar nesse intervalo</returns>
        Task<IEnumerable<OrderRent>> GetRentsEndingSoon(DateTime start, DateTime end);
        Task<Contract> GetContractByOrderRentId(Guid orderRentId);
    }
}
