using AutoMapper;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Interfaces.Parameters;
using LojaMaquinas.WebApp.ViewModels.MVC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LojaMaquinas.WebApp.Controllers.MVC
{
    /// <summary>
    /// Controlador da dashboard principal.
    /// Apresenta métricas de vendas, alugueres e stock.
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class DashboardController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly IStockRepository _stockRepository;

        /// <summary>
        /// Construtor do controlador de dashboard.
        /// </summary>
        /// <param name="mapper">Serviço de mapeamento AutoMapper</param>
        /// <param name="notifier">Serviço de notificações</param>
        /// <param name="orderRepository">Repositório de pedidos</param>
        /// <param name="stockRepository">Repositório de stock</param>
        public DashboardController(
            IMapper mapper,
            INotifierService notifier,
            IOrderRepository orderRepository,
            IStockRepository stockRepository) : base(notifier)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _stockRepository = stockRepository;
        }

        #region GET Methods

        /// <summary>
        /// Apresenta a dashboard principal com métricas agregadas.
        /// </summary>
        /// <returns>View com DashboardViewModel</returns>
        [Route("dashboard")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var viewModel = new DashboardViewModel();

                // Obter data atual
                var currentDate = DateTime.Now;
                var currentYear = currentDate.Year;
                var currentMonth = currentDate.Month;

                #region Vendas Mensais

                // Receita do mês atual
                viewModel.CurrentMonthRevenue = await _orderRepository.GetMonthlySalesRevenue(currentYear, currentMonth);

                // Receita do mês anterior
                var previousMonth = currentDate.AddMonths(-1);
                viewModel.PreviousMonthRevenue = await _orderRepository.GetMonthlySalesRevenue(previousMonth.Year, previousMonth.Month);

                // Calcular percentagem de variação mensal
                if (viewModel.PreviousMonthRevenue > 0)
                {
                    viewModel.MonthlyRevenueChangePercent =
                        ((viewModel.CurrentMonthRevenue - viewModel.PreviousMonthRevenue) / viewModel.PreviousMonthRevenue) * 100;
                }
                else
                {
                    viewModel.MonthlyRevenueChangePercent = viewModel.CurrentMonthRevenue > 0 ? 100 : 0;
                }

                // Dados para gráfico de vendas mensais (últimos 6 meses)
                viewModel.MonthlyRevenueData = await _orderRepository.GetMonthlyRevenueData(6);

                #endregion

                #region Alugueres Semanais

                // Calcular início e fim da semana atual (domingo a sábado)
                var dayOfWeek = (int)currentDate.DayOfWeek;
                var currentWeekStart = currentDate.AddDays(-dayOfWeek).Date;
                var currentWeekEnd = currentWeekStart.AddDays(6).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

                // Alugueres da semana atual
                viewModel.CurrentWeekRentals = await _orderRepository.GetWeeklyRentalsCount(currentWeekStart, currentWeekEnd);

                // Calcular semana anterior
                var previousWeekStart = currentWeekStart.AddDays(-7);
                var previousWeekEnd = currentWeekEnd.AddDays(-7);

                // Alugueres da semana anterior
                viewModel.PreviousWeekRentals = await _orderRepository.GetWeeklyRentalsCount(previousWeekStart, previousWeekEnd);

                // Calcular percentagem de variação semanal
                if (viewModel.PreviousWeekRentals > 0)
                {
                    viewModel.WeeklyRentalsChangePercent =
                        ((decimal)(viewModel.CurrentWeekRentals - viewModel.PreviousWeekRentals) / viewModel.PreviousWeekRentals) * 100;
                }
                else
                {
                    viewModel.WeeklyRentalsChangePercent = viewModel.CurrentWeekRentals > 0 ? 100 : 0;
                }

                // Dados para gráfico de alugueres semanais (últimas 8 semanas)
                viewModel.WeeklyRentalsData = await _orderRepository.GetWeeklyRentalsData(8);

                #endregion

                #region Stock de Máquinas

                // Total de máquinas
                viewModel.TotalMachines = await _stockRepository.GetTotalStockCount();

                // Máquinas disponíveis
                viewModel.AvailableMachines = await _stockRepository.GetAvailableStockCount();

                // Máquinas em manutenção
                viewModel.MachinesInMaintenance = await _stockRepository.GetStockInMaintenanceCount();

                #endregion

                return View(viewModel);
            }
            catch (Exception ex)
            {
                var fullError = $"Erro: {ex.Message} | InnerException: {ex.InnerException?.Message}";
                return Json(new { success = false, message = "", errors = new[] { fullError } });
            }
        }

        #endregion
    }
}
