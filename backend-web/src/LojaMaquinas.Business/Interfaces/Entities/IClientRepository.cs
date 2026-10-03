using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.Business.Interfaces.Entities
{
    /// <summary>
    /// Interface de repositório para gestão de clientes na base de dados.
    /// Fornece operações específicas para consulta de clientes e suas relações.
    /// </summary>
    public interface IClientRepository : IRepository<Client>
    {
        /// <summary>
        /// Obtém um cliente com os dados do utilizador associado (User).
        /// </summary>
        /// <param name="id">Identificador único do cliente</param>
        /// <returns>Cliente com a relação User carregada</returns>
        Task<Client> GetClientWithUser(Guid id);

        /// <summary>
        /// Obtém um cliente com todos os seus pedidos (Orders).
        /// </summary>
        /// <param name="id">Identificador único do cliente</param>
        /// <returns>Cliente com a coleção de Orders carregada</returns>
        Task<Client> GetClientWithOrders(Guid id);

        /// <summary>
        /// Obtém um cliente com o utilizador e todos os pedidos associados.
        /// </summary>
        /// <param name="id">Identificador único do cliente</param>
        /// <returns>Cliente com User e Orders carregados</returns>
        Task<Client> GetClientWithUserAndOrders(Guid id);

        /// <summary>
        /// Obtém todos os clientes com os dados dos utilizadores associados.
        /// </summary>
        /// <returns>Coleção de clientes com a relação User carregada</returns>
        Task<IEnumerable<Client>> GetAllClientsWithUsers();

        /// <summary>
        /// Obtém um cliente pelo seu número de identificação fiscal (NIF).
        /// </summary>
        /// <param name="nif">Número de Identificação Fiscal</param>
        /// <returns>Cliente com o NIF especificado ou null se não existir</returns>
        Task<Client> GetByNif(string nif);

        /// <summary>
        /// Obtém um cliente pelo identificador do utilizador associado.
        /// </summary>
        /// <param name="userId">Identificador único do User</param>
        /// <returns>Cliente associado ao utilizador ou null se não existir</returns>
        Task<Client> GetByUserId(Guid userId);
    }
}
