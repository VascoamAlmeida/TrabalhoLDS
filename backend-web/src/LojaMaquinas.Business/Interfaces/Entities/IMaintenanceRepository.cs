using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.Business.Interfaces.Entities
{
    /// <summary>
    /// Interface de repositório para gestão de manutenções na base de dados.
    /// Fornece operações específicas para consulta de histórico de manutenções.
    /// </summary>
    public interface IMaintenanceRepository : IRepository<Maintenance>
    {
        /// <summary>
        /// Obtém todas as manutenções com os pedidos associados.
        /// </summary>
        /// <returns>Coleção de todas as manutenções com Orders carregados</returns>
        Task<IEnumerable<Maintenance>> GetAllMaintenancesWithOrders();

        /// <summary>
        /// Obtém todas as manutenções associadas a um cliente através dos seus pedidos.
        /// Útil para histórico de manutenções do cliente.
        /// </summary>
        /// <param name="clientId">Identificador único do cliente</param>
        /// <returns>Coleção de manutenções do cliente</returns>
        Task<IEnumerable<Maintenance>> GetMaintenancesByClient(Guid clientId);

        /// <summary>
        /// Obtém todas as manutenções de um pedido específico.
        /// </summary>
        /// <param name="orderId">Identificador único do pedido</param>
        /// <returns>Coleção de manutenções do pedido</returns>
        Task<IEnumerable<Maintenance>> GetMaintenancesByOrder(Guid orderId);

        /// <summary>
        /// Obtém uma manutenção com todos os detalhes incluindo Order e Client.
        /// </summary>
        /// <param name="id">Identificador único da manutenção</param>
        /// <returns>Manutenção com todas as relações carregadas</returns>
        Task<Maintenance> GetMaintenanceWithDetails(Guid id);

        /// <summary>
        /// Obtém todas as manutenções com um determinado status.
        /// Útil para atualização automática de status.
        /// </summary>
        /// <param name="status">Status da manutenção (Pending, InProgress, Completed, Cancelled)</param>
        /// <returns>Coleção de manutenções com o status especificado</returns>
        Task<IEnumerable<Maintenance>> GetMaintenancesByStatus(Models.Enums.MaintenanceStatus status);
    }
}
