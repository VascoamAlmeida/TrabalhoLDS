using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LojaMaquinas.Data.Repositories.Entities
{
    /// <summary>
    /// Repositório para acesso aos dados de pedidos na base de dados.
    /// Fornece métodos para gestão de pedidos de compra e aluguer.
    /// </summary>
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        /// <summary>
        /// Inicializa uma nova instância do repositório de pedidos.
        /// </summary>
        /// <param name="context">Contexto da base de dados</param>
        public OrderRepository(MyDbContext context) : base(context) { }

        /// <summary>
        /// Obtém todos os pedidos com os seus clientes associados, ordenados por ClientId.
        /// </summary>
        /// <returns>Coleção de pedidos com Clients carregados</returns>
        public async Task<IEnumerable<Order>> GetOrdersAndUsers()
        {
            return await Db.E_Orders.AsNoTracking()
                .Include(f => f.Client).OrderBy(o => o.ClientId).ToListAsync();
        }

        /// <summary>
        /// Obtém todos os pedidos de um cliente específico.
        /// </summary>
        /// <param name="clientId">Identificador único do cliente</param>
        /// <returns>Coleção de pedidos do cliente</returns>
        public async Task<IEnumerable<Order>> GetOrdersByClientId(Guid clientId)
        {
            return await Find(o => o.ClientId == clientId);
        }

        /// <summary>
        /// Obtém um pedido com cliente e stock associados (eager loading).
        /// </summary>
        /// <param name="id">Identificador único do pedido</param>
        /// <returns>Pedido com Client e Stock carregados ou null se não existir</returns>
        public async Task<IEnumerable<Order>> GetOrdersAndClientAndStock()
        {
            return await Db.E_Orders.AsNoTracking()
                           .Include(c => c.Client)
                           .Include(s => s.Stock)
                           .ThenInclude(os => os.Product)
                           .ToListAsync();
        }

        /// <summary>
        /// Obtém um pedido com cliente e stock associados (eager loading).
        /// </summary>
        /// <param name="id">Identificador único do pedido</param>
        /// <returns>Pedido com Client e Stock carregados ou null se não existir</returns>
        public async Task<Order> GetOrderAndClientAndStockByOrderId(Guid id)
        {
            return await Db.E_Orders.AsNoTracking()
                           .Include(c => c.Client)
                                .ThenInclude(c => c.User)
                           .Include(s => s.Stock)
                                .ThenInclude(os => os.Product)
                           .FirstOrDefaultAsync(o => o.Id == id);
        }

        /// <summary>
        /// Obtém o aluguer (OrderRent) associado a um pedido específico.
        /// </summary>
        /// <param name="orderId">Identificador único do pedido</param>
        /// <returns>OrderRent associado ao pedido ou null se não existir</returns>
        public async Task<OrderRent> GetOrderRentByOrderId(Guid orderId)
        {
            return await Db.E_OrderRents.AsNoTracking()
                                        .Where(o => o.OrderId == orderId)
                                        .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Remove um aluguer (OrderRent) específico da base de dados.
        /// </summary>
        /// <param name="orderRent">Entidade OrderRent a remover</param>
        public async Task RemoveOrderRentByOrder(OrderRent orderRent)
        {
            Db.E_OrderRents.Remove(orderRent);
            //await SaveChanges();
        }

        /// <summary>
        /// Obtém todos os pedidos de um cliente específico.
        /// </summary>
        /// <param name="clientId">Identificador único do cliente</param>
        /// <returns>Coleção de pedidos do cliente</returns>
        public async Task<IEnumerable<Order>> GetOrdersAndStockByClientId(Guid clientId)
        {
            return await Db.E_Orders.AsNoTracking()
                           .Include(c => c.Client)
                           .Include(s => s.Stock)
                                .ThenInclude(os => os.Product)
                                .Where(o => o.ClientId == clientId)
                           .ToListAsync();
        }

        /// <summary>
        /// Obtém o valor total de receita de vendas (Sale) para um mês específico.
        /// </summary>
        /// <param name="year">Ano da consulta</param>
        /// <param name="month">Mês da consulta (1-12)</param>
        /// <returns>Valor total de receita das vendas do mês especificado</returns>
        public async Task<decimal> GetMonthlySalesRevenue(int year, int month)
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            return await Db.E_Orders.AsNoTracking()
                .Where(o => o.Type == OrderType.Sale
                         && o.OrderDate >= startDate
                         && o.OrderDate <= endDate)
                .SumAsync(o => o.Total);
        }

        /// <summary>
        /// Obtém os dados de receita de vendas mensais para os últimos N meses.
        /// Útil para construir gráficos de evolução de vendas.
        /// </summary>
        /// <param name="numberOfMonths">Número de meses a consultar (incluindo o mês atual)</param>
        /// <returns>Dicionário com nome do mês (abreviado) e valor de receita correspondente</returns>
        public async Task<Dictionary<string, decimal>> GetMonthlyRevenueData(int numberOfMonths)
        {
            var result = new Dictionary<string, decimal>();
            var currentDate = DateTime.Now;

            for (int i = numberOfMonths - 1; i >= 0; i--)
            {
                var targetDate = currentDate.AddMonths(-i);
                var monthRevenue = await GetMonthlySalesRevenue(targetDate.Year, targetDate.Month);
                result.Add(targetDate.ToString("MMM"), monthRevenue);
            }

            return result;
        }

        /// <summary>
        /// Obtém o número de alugueres (Rent) num período específico.
        /// </summary>
        /// <param name="startDate">Data de início do período</param>
        /// <param name="endDate">Data de fim do período</param>
        /// <returns>Número total de alugueres no período especificado</returns>
        public async Task<int> GetWeeklyRentalsCount(DateTime startDate, DateTime endDate)
        {
            return await Db.E_Orders.AsNoTracking()
                .Where(o => o.Type == OrderType.Rent
                         && o.OrderDate >= startDate
                         && o.OrderDate <= endDate)
                .CountAsync();
        }

        /// <summary>
        /// Obtém os dados de alugueres semanais para as últimas N semanas.
        /// Útil para construir gráficos de evolução de alugueres.
        /// </summary>
        /// <param name="numberOfWeeks">Número de semanas a consultar (incluindo a semana atual)</param>
        /// <returns>Lista com o número de alugueres de cada semana</returns>
        public async Task<List<int>> GetWeeklyRentalsData(int numberOfWeeks)
        {
            var result = new List<int>();
            var currentDate = DateTime.Now;

            for (int i = numberOfWeeks - 1; i >= 0; i--)
            {
                var weekStart = currentDate.AddDays(-(7 * i) - (int)currentDate.DayOfWeek);
                var weekEnd = weekStart.AddDays(6);
                var count = await GetWeeklyRentalsCount(weekStart, weekEnd);
                result.Add(count);
            }

            return result;
        }
    }
}
