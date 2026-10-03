using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LojaMaquinas.Data.Repositories.Entities
{
    /// <summary>
    /// Repositório para acesso aos dados de contratos na base de dados.
    /// Fornece métodos para gestão de contratos de aluguer.
    /// </summary>
    public class ContractRepository : Repository<Contract>, IContractRepository
    {
        /// <summary>
        /// Inicializa uma nova instância do repositório de contratos.
        /// </summary>
        /// <param name="context">Contexto da base de dados</param>
        public ContractRepository(MyDbContext context) : base(context) { }

        /// <summary>
        /// Obtém o contrato associado a um aluguer específico (pelo OrderRentId).
        /// </summary>
        /// <param name="contractId">Identificador único do OrderRent</param>
        /// <returns>Contrato com OrderRent carregado ou null se não existir</returns>
        public async Task<Contract> GetOrderRentIDbyContract(Guid contractId)
        {
            return await Db.E_Contracts
                            .AsNoTracking()
                            .Include(c => c.OrderRent)
                                .ThenInclude(or => or.Order)
                                    .ThenInclude(o => o.Client)
									    .ThenInclude(cl => cl.User)
							.Include(c => c.OrderRent)
								.ThenInclude(or => or.Order)
									.ThenInclude(o => o.Stock)
									    .ThenInclude(o => o.Product)
							.FirstOrDefaultAsync(c => c.Id == contractId);
        }

        /// <summary>
        /// Obtém os contratos associados a um cliente.
        /// </summary>
        /// <param name="email">Email do cliente</param>
        /// <returns>Contratos associados ao cliente</returns>
        public async Task<IEnumerable<Contract>> GetAllByClientEmail(string email)
        {
            return await Db.E_Contracts
                .Include(c => c.OrderRent)
                    .ThenInclude(or => or.Order)
                        .ThenInclude(o => o.Client)
                            .ThenInclude(c => c.User)
                .Where(c => c.OrderRent.Order.Client.User.Email == email)
                .ToListAsync();
        }
    }
}
