using LojaMaquinas.Business.Interfaces.Parameters;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LojaMaquinas.Data.Repositories.Parameters
{
    /// <summary>
    /// Repositório para acesso aos dados de stock/inventário na base de dados.
    /// Fornece métodos para gestão de items individuais de stock e suas relações.
    /// </summary>
    public class StockRepository : Repository<Stock>, IStockRepository
    {
        /// <summary>
        /// Remove um item de stock específico (usando a entidade completa).
        /// Útil quando já temos a entidade carregada e queremos removê-la.
        /// </summary>
        /// <param name="stock">Entidade de stock a remover</param>
        public async Task RemoveSpecific(Stock stock)
        {
            DbSet.Remove(stock);
            await SaveChanges();
        }

        /// <summary>
        /// Inicializa uma nova instância do repositório de stock.
        /// </summary>
        /// <param name="context">Contexto da base de dados</param>
        public StockRepository(MyDbContext context) : base(context) { }

        /// <summary>
        /// Obtém um item de stock com pedidos e produto associados (eager loading).
        /// </summary>
        /// <param name="id">Identificador único do stock</param>
        /// <returns>Stock com Orders e Product carregados ou null se não existir</returns>
        public async Task<Stock> GetStockWithOrdersAndProduct(Guid id)
        {
            return await Db.E_Stocks.AsNoTracking()
                                     .Include(s => s.Orders)
                                     .Include(s => s.Product)
                                     .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Devolve todos os items com o produto associado.
        /// </summary>
        /// <returns>Stocks com Product carregado</returns>
        public async Task<IEnumerable<Stock>> GetAllStocksWithProduct()
        {
            return await Db.E_Stocks.AsNoTracking()
                                     .Include(s => s.Product)
                                     .ToListAsync();
        }

        /// <summary>
        /// Obtém um item de stock com todos os seus pedidos associados.
        /// </summary>
        /// <param name="id">Identificador único do stock</param>
        /// <returns>Stock com Orders carregados ou null se não existir</returns>
        public async Task<Stock> GetOrdersByStockId(Guid id)
        {
            return await Db.E_Stocks.AsNoTracking()
                                     .Include(s => s.Orders)
                                     .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Obtém todos os items de stock de um produto específico.
        /// </summary>
        /// <param name="productId">Identificador único do produto</param>
        /// <returns>Coleção de items de stock do produto</returns>
        public async Task<IEnumerable<Stock>> GetStocksByProduct(Guid productId)
        {
            return await Db.E_Stocks.AsNoTracking()
                                     .Where(s => s.ProductId == productId)
                                     .ToListAsync();
        }

        /// <summary>
        /// Obtém um item de stock com todos os detalhes completos (produto, categoria e pedidos).
        /// Inclui ThenInclude para carregar a categoria do produto.
        /// </summary>
        /// <param name="id">Identificador único do stock</param>
        /// <returns>Stock com Product, Category e Orders carregados ou null se não existir</returns>
        public async Task<Stock> GetStockWithAllDetails(Guid id)
        {
            return await Db.E_Stocks.AsNoTracking()
                                     .Include(s => s.Product)
                                         .ThenInclude(p => p.Category)
                                     .Include(s => s.Orders)
                                     .FirstOrDefaultAsync(s => s.Id == id);
        }

        /// <summary>
        /// Obtém todos os items de stock filtrados por estado (Available, SoldOut, NotAvailable).
        /// </summary>
        /// <param name="status">Estado de stock a filtrar</param>
        /// <returns>Coleção de items de stock com o estado especificado</returns>
        public async Task<IEnumerable<Stock>> GetStocksByStatus(StockStatus status)
        {
            return await Db.E_Stocks.AsNoTracking()
                                     .Where(s => s.StockStatus == status)
                                     .ToListAsync();
        }

        /// <summary>
        /// Obtém o número total de items de stock existentes.
        /// </summary>
        /// <returns>Contagem total de items de stock</returns>
        public async Task<int> GetTotalStockCount()
        {
            return await Db.E_Stocks.AsNoTracking().CountAsync();
        }

        /// <summary>
        /// Obtém o número de items de stock disponíveis para venda ou aluguer.
        /// </summary>
        /// <returns>Contagem de items de stock com estado Available</returns>
        public async Task<int> GetAvailableStockCount()
        {
            return await Db.E_Stocks.AsNoTracking()
                .Where(s => s.StockStatus == StockStatus.Available)
                .CountAsync();
        }

        /// <summary>
        /// Obtém o número de items de stock que estão atualmente em manutenção.
        /// Considera items com manutenções no estado InProgress ou Pending.
        /// </summary>
        /// <returns>Contagem de items de stock em manutenção</returns>
        public async Task<int> GetStockInMaintenanceCount()
        {
            return await Db.E_Stocks.AsNoTracking()
                .Where(s => Db.E_Maintenances.Any(m =>
                    m.OrderId == s.Orders.FirstOrDefault().Id
                    && (m.Status == MaintenanceStatus.InProgress
                        || m.Status == MaintenanceStatus.Pending)))
                .CountAsync();
        }
    }
}

