using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LojaMaquinas.Data.Repositories.Entities
{
    /// <summary>
    /// Repositório para acesso aos dados de alugueres na base de dados.
    /// Fornece métodos para gestão de OrderRent e relações com contratos.
    /// </summary>
    public class OrderRentRepository : Repository<OrderRent>, IOrderRentRepository
    {
        /// <summary>
        /// Inicializa uma nova instância do repositório de alugueres.
        /// </summary>
        /// <param name="context">Contexto da base de dados</param>
        public OrderRentRepository(MyDbContext context) : base(context) { }

        /// <summary>
        /// Remove um contrato específico da base de dados.
        /// </summary>
        /// <param name="contract">Entidade de contrato a remover</param>
        public async Task DeleteContractByOrderRent(Contract contract)
        {
            Db.E_Contracts.Remove(contract);
            //await SaveChanges();
        }

        /// <summary>
        /// Obtém um aluguer com o seu contrato associado.
        /// </summary>
        /// <param name="orderRentId">Identificador único do aluguer</param>
        /// <returns>OrderRent com Contract carregado ou null se não existir</returns>
        public async Task<OrderRent> GetOrderAndRentOrderByOrderId(Guid orderRentId)
        {
            return await Db.E_OrderRents
                           .AsNoTracking()
                           .Include(or => or.Contract)
                           .FirstOrDefaultAsync(or => or.Id == orderRentId);
        }

        /// <summary>
        /// Obtém um aluguer com o seu contrato associado (método duplicado).
        /// </summary>
        /// <param name="orderRentId">Identificador único do aluguer</param>
        /// <returns>OrderRent com Contract carregado ou null se não existir</returns>
        public async Task<OrderRent> GetOrderRentAndContractByOrderRentId(Guid orderRentId)
        {
            return await Db.E_OrderRents
                           .AsNoTracking()
                           .Include(or => or.Contract)
                           .FirstOrDefaultAsync(or => or.Id == orderRentId);
        }

        /// <summary>
        /// Obtém um aluguer com pedido e contrato associados (eager loading completo).
        /// </summary>
        /// <param name="orderRentId">Identificador único do aluguer</param>
        /// <returns>OrderRent com Order e Contract carregados ou null se não existir</returns>
        public async Task<OrderRent> GetOrderRentAndOrderAndContractByOrderRentId(Guid orderRentId)
        {
			return await Db.E_OrderRents
				.AsNoTracking()
				.Include(or => or.Order)
					.ThenInclude(o => o.Client)
						.ThenInclude(c => c.User)
				.Include(or => or.Order)
					.ThenInclude(o => o.Stock)
						.ThenInclude(s => s.Product)
				.Include(or => or.Contract)
				.FirstOrDefaultAsync(or => or.Id == orderRentId);
		}

        /// <summary>
        /// Para obter as OrderRent entre certas datas
        /// </summary>
        /// <param name="start">Data de inicio</param>
        /// <param name="end">Data de fim</param>
        /// <returns>OrderRent entre essas datas</returns>
        public async Task<IEnumerable<OrderRent>> GetRentsEndingSoon(DateTime start, DateTime end)
        {
            return await Db.E_OrderRents
                .AsNoTracking()
                .Where(or =>
                    or.EndDate >= start &&
                    or.EndDate <= end &&
                    (or.Status == OrderRentStatus.Active ||
                     or.Status == OrderRentStatus.Approved))
                .ToListAsync();
        }

        public async Task<Contract> GetContractByOrderRentId(Guid orderRentId)
        {
            return await Db.E_Contracts.AsNoTracking()
                         .Where(or => or.OrderRentId == orderRentId)
                         .FirstOrDefaultAsync();
        }
    }
}
