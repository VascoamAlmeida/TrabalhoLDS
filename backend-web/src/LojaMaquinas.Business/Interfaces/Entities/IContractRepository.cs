using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.Business.Interfaces.Entities
{
    /// <summary>
    /// Interface de repositório para gestão de contratos na base de dados.
    /// Fornece operações específicas para consulta de contratos e suas relações.
    /// </summary>
    public interface IContractRepository : IRepository<Contract>
    {
        /// <summary>
        /// Obtém o contrato associado a um aluguel (OrderRent).
        /// </summary>
        /// <param name="contractId">Identificador único do OrderRent</param>
        /// <returns>Contrato associado ao aluguel ou null se não existir</returns>
        Task<Contract> GetOrderRentIDbyContract(Guid contractId);

        /// <summary>
        /// Obtém os contratos associados a um cliente.
        /// </summary>
        /// <param name="email">Email do cliente</param>
        /// <returns>Contratos associados ao cliente</returns>
        Task<IEnumerable<Contract>> GetAllByClientEmail(String email);
    }
}
