using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LojaMaquinas.Data.Repositories.Entities
{
    /// <summary>
    /// Repositório para acesso aos dados de manutenções na base de dados.
    /// Fornece métodos avançados para consulta de histórico e detalhes de manutenções.
    /// </summary>
    public class MaintenanceRepository : Repository<Maintenance>, IMaintenanceRepository
    {
        /// <summary>
        /// Inicializa uma nova instância do repositório de manutenções.
        /// </summary>
        /// <param name="context">Contexto da base de dados</param>
        public MaintenanceRepository(MyDbContext context) : base(context) { }

        /// <summary>
        /// Obtém todas as manutenções com os pedidos associados.
        /// </summary>
        /// <returns>Coleção de todas as manutenções com Orders carregados</returns>
        public async Task<IEnumerable<Maintenance>> GetAllMaintenancesWithOrders()
        {
            return await Db.E_Maintenances.AsNoTracking()
                                           .Include(m => m.Order)
                                           .OrderByDescending(m => m.ScheduledDate)
                                           .ToListAsync();
        }

        /// <summary>
        /// Obtém o histórico de manutenções de um cliente (através dos seus pedidos).
        /// Inclui Order, Stock e Product. Ordenado por data de agendamento (mais recentes primeiro).
        /// </summary>
        /// <param name="clientId">Identificador único do cliente</param>
        /// <returns>Coleção de manutenções do cliente ordenadas por data</returns>
        public async Task<IEnumerable<Maintenance>> GetMaintenancesByClient(Guid clientId)
        {
            return await Db.E_Maintenances.AsNoTracking()
                                           .Include(m => m.Order)
                                               .ThenInclude(o => o.Stock)
                                                   .ThenInclude(s => s.Product)
                                           .Where(m => m.Order.ClientId == clientId)
                                           .OrderByDescending(m => m.ScheduledDate)
                                           .ToListAsync();
        }

        /// <summary>
        /// Obtém todas as manutenções de um pedido específico.
        /// Ordenado por data de agendamento (mais recentes primeiro).
        /// </summary>
        /// <param name="orderId">Identificador único do pedido</param>
        /// <returns>Coleção de manutenções do pedido ordenadas por data</returns>
        public async Task<IEnumerable<Maintenance>> GetMaintenancesByOrder(Guid orderId)
        {
            return await Db.E_Maintenances.AsNoTracking()
                                           .Include(m => m.Order)
                                           .Where(m => m.OrderId == orderId)
                                           .OrderByDescending(m => m.ScheduledDate)
                                           .ToListAsync();
        }

        /// <summary>
        /// Obtém uma manutenção com todos os detalhes completos (Order, Client, User, Stock, Product).
        /// Carrega toda a hierarquia de relações para visualização detalhada.
        /// </summary>
        /// <param name="id">Identificador único da manutenção</param>
        /// <returns>Manutenção com todas as relações carregadas ou null se não existir</returns>
        public async Task<Maintenance> GetMaintenanceWithDetails(Guid id)
        {
            return await Db.E_Maintenances.AsNoTracking()
                                           .Include(m => m.Order)
                                               .ThenInclude(o => o.Client)
                                                   .ThenInclude(c => c.User)
                                           .Include(m => m.Order)
                                               .ThenInclude(o => o.Stock)
                                                   .ThenInclude(s => s.Product)
                                           .FirstOrDefaultAsync(m => m.Id == id);
        }

        /// <summary>
        /// Obtém todas as manutenções com um determinado status.
        /// Útil para atualização automática de status (ex: obter todas Pending para verificar se devem passar para InProgress).
        /// </summary>
        /// <param name="status">Status da manutenção (Pending, InProgress, Completed, Cancelled)</param>
        /// <returns>Coleção de manutenções com o status especificado</returns>
        public async Task<IEnumerable<Maintenance>> GetMaintenancesByStatus(Business.Models.Enums.MaintenanceStatus status)
        {
            return await Db.E_Maintenances
                           .Where(m => m.Status == status)
                           .OrderBy(m => m.ScheduledDate)
                           .ToListAsync();
        }
    }
}
