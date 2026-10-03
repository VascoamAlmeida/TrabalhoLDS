using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LojaMaquinas.Data.Repositories.Entities
{
    /// <summary>
    /// Repositório para acesso aos dados de clientes na base de dados.
    /// Fornece métodos específicos para queries com eager loading de relações.
    /// </summary>
    public class ClientRepository : Repository<Client>, IClientRepository
    {
        /// <summary>
        /// Inicializa uma nova instância do repositório de clientes.
        /// </summary>
        /// <param name="context">Contexto da base de dados</param>
        public ClientRepository(MyDbContext context) : base(context) { }

        /// <summary>
        /// Obtém um cliente com o seu utilizador associado (eager loading).
        /// </summary>
        /// <param name="id">Identificador único do cliente</param>
        /// <returns>Cliente com User carregado ou null se não existir</returns>
        public async Task<Client> GetClientWithUser(Guid id)
        {
            return await Db.E_Clients.AsNoTracking()
                                      .Include(c => c.User)
                                      .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Obtém um cliente com todos os seus pedidos (eager loading).
        /// Útil para verificar histórico de compras e alugueres.
        /// </summary>
        /// <param name="id">Identificador único do cliente</param>
        /// <returns>Cliente com Orders carregados ou null se não existir</returns>
        public async Task<Client> GetClientWithOrders(Guid id)
        {
            return await Db.E_Clients.AsNoTracking()
                                      .Include(c => c.Orders)
                                      .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Obtém um cliente com utilizador e pedidos (eager loading completo).
        /// Carrega todas as relações principais do cliente.
        /// </summary>
        /// <param name="id">Identificador único do cliente</param>
        /// <returns>Cliente com User e Orders carregados ou null se não existir</returns>
        public async Task<Client> GetClientWithUserAndOrders(Guid id)
        {
            return await Db.E_Clients.AsNoTracking()
                                      .Include(c => c.User)
                                      .Include(c => c.Orders)
                                      .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Obtém todos os clientes com os seus utilizadores associados.
        /// </summary>
        /// <returns>Coleção de clientes com Users carregados</returns>
        public async Task<IEnumerable<Client>> GetAllClientsWithUsers()
        {
            return await Db.E_Clients.AsNoTracking()
                                      .Include(c => c.User)
                                      .ToListAsync();
        }

        /// <summary>
        /// Procura um cliente pelo seu NIF (Número de Identificação Fiscal).
        /// </summary>
        /// <param name="nif">NIF do cliente</param>
        /// <returns>Cliente encontrado ou null se não existir</returns>
        public async Task<Client> GetByNif(string nif)
        {
            return await Db.E_Clients.AsNoTracking()
                                      .FirstOrDefaultAsync(c => c.NIF == nif);
        }

        /// <summary>
        /// Obtém o cliente associado a um utilizador específico.
        /// </summary>
        /// <param name="userId">Identificador único do utilizador</param>
        /// <returns>Cliente encontrado ou null se não existir</returns>
        public async Task<Client> GetByUserId(Guid userId)
        {
            return await Db.E_Clients.AsNoTracking()
                                      .FirstOrDefaultAsync(c => c.UserId == userId);
        }
    }
}
