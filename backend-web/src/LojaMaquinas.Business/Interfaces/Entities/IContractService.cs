namespace LojaMaquinas.Business.Interfaces.Entities
{
    /// <summary>
    /// Interface de serviço para gestão de contratos de aluguel.
    /// Contém a lógica de negócio para criação e gestão de contratos.
    /// </summary>
    public interface IContractService : IDisposable
    {
        /// <summary>
        /// Gera um novo contrato a partir de um aluguel (OrderRent).
        /// Obtém os termos e condições do sistema e cria o contrato.
        /// </summary>
        /// <param name="orderRentId">Identificador único do OrderRent</param>
        /// <returns>Identificador do contrato criado ou Guid.Empty em caso de erro</returns>
        Task<Guid> Add(Guid orderRentId);

        /// <summary>
        /// Atualiza os termos e condições de um contrato existente.
        /// </summary>
        /// <param name="contractId">Identificador único do contrato</param>
        /// <param name="terms">Novos termos e condições do contrato</param>
        Task Update(Guid contractId, string terms);

        /// <summary>
        /// Remove um contrato do sistema.
        /// </summary>
        /// <param name="contractId">Identificador único do contrato a remover</param>
        Task Remove(Guid contractId);
    }
}

