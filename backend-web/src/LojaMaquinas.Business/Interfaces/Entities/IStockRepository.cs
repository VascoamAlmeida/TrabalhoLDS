using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;

namespace LojaMaquinas.Business.Interfaces.Parameters
{
    /// <summary>
    /// Interface de repositório para gestão de inventário (Stock) na base de dados.
    /// Fornece operações específicas para consulta e manipulação de items de stock.
    /// </summary>
    public interface IStockRepository : IRepository<Stock>
    {
        /// <summary>
        /// Remove uma entidade de stock específica da base de dados.
        /// </summary>
        /// <param name="stock">Entidade Stock a ser removida</param>
        Task RemoveSpecific(Stock stock);

        /// <summary>
        /// Obtém um stock com os pedidos e produto associados.
        /// </summary>
        /// <param name="id">Identificador único do stock</param>
        /// <returns>Stock com relações Orders e Product carregadas</returns>
        Task<Stock> GetStockWithOrdersAndProduct(Guid id);

        /// <summary>
        /// Obtém um stock com todos os pedidos associados.
        /// </summary>
        /// <param name="id">Identificador único do stock</param>
        /// <returns>Stock com a coleção de Orders carregada</returns>
        Task<Stock> GetOrdersByStockId(Guid id);

        /// <summary>
        /// Obtém todos os stocks de um produto específico.
        /// </summary>
        /// <param name="productId">Identificador único do produto</param>
        /// <returns>Coleção de stocks do produto</returns>
        Task<IEnumerable<Stock>> GetStocksByProduct(Guid productId);

        /// <summary>
        /// Obtém um stock com todos os detalhes e relações carregadas (eager loading).
        /// </summary>
        /// <param name="id">Identificador único do stock</param>
        /// <returns>Stock com todas as relações carregadas</returns>
        Task<Stock> GetStockWithAllDetails(Guid id);

        /// <summary>
        /// Obtém todos os stocks com um estado específico.
        /// </summary>
        /// <param name="status">Estado do stock (Available, SoldOut, NotAvailable)</param>
        /// <returns>Coleção de stocks com o estado especificado</returns>
        Task<IEnumerable<Stock>> GetStocksByStatus(StockStatus status);

        /// <summary>
        /// Devolve todos os items com o produto associado.
        /// </summary>
        /// <returns>Stocks com Product carregado</returns>
        Task<IEnumerable<Stock>> GetAllStocksWithProduct();

        /// <summary>
        /// Obtém o número total de items de stock existentes.
        /// </summary>
        /// <returns>Contagem total de items de stock</returns>
        Task<int> GetTotalStockCount();

        /// <summary>
        /// Obtém o número de items de stock disponíveis para venda ou aluguer.
        /// </summary>
        /// <returns>Contagem de items de stock com estado Available</returns>
        Task<int> GetAvailableStockCount();

        /// <summary>
        /// Obtém o número de items de stock que estão atualmente em manutenção.
        /// Considera items com manutenções no estado InProgress ou Pending.
        /// </summary>
        /// <returns>Contagem de items de stock em manutenção</returns>
        Task<int> GetStockInMaintenanceCount();
    }
}
