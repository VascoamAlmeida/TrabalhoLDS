using System.ComponentModel;

namespace LojaMaquinas.WebApp.ViewModels.MVC
{
    /// <summary>
    /// ViewModel para a dashboard principal.
    /// Contém dados agregados de vendas, alugueres e stock.
    /// </summary>
    public class DashboardViewModel
    {
        #region Vendas Mensais

        /// <summary>
        /// Receita total de vendas do mês atual.
        /// </summary>
        [DisplayName("Vendas Mensais")]
        public decimal CurrentMonthRevenue { get; set; }

        /// <summary>
        /// Receita total de vendas do mês anterior.
        /// </summary>
        [DisplayName("Vendas Mês Anterior")]
        public decimal PreviousMonthRevenue { get; set; }

        /// <summary>
        /// Percentagem de crescimento/decréscimo de vendas em relação ao mês anterior.
        /// Valor positivo indica crescimento, negativo indica decréscimo.
        /// </summary>
        [DisplayName("Variação Mensal")]
        public decimal MonthlyRevenueChangePercent { get; set; }

        /// <summary>
        /// Dados de receita mensal para gráfico.
        /// Chave: Nome do mês (ex: "Jan", "Fev"), Valor: Receita do mês.
        /// </summary>
        public Dictionary<string, decimal> MonthlyRevenueData { get; set; }

        #endregion

        #region Alugueres Semanais

        /// <summary>
        /// Número de alugueres realizados na semana atual.
        /// </summary>
        [DisplayName("Alugueres Semanais")]
        public int CurrentWeekRentals { get; set; }

        /// <summary>
        /// Número de alugueres realizados na semana anterior.
        /// </summary>
        [DisplayName("Alugueres Semana Anterior")]
        public int PreviousWeekRentals { get; set; }

        /// <summary>
        /// Percentagem de variação de alugueres em relação à semana anterior.
        /// Valor positivo indica crescimento, negativo indica decréscimo.
        /// </summary>
        [DisplayName("Variação Semanal")]
        public decimal WeeklyRentalsChangePercent { get; set; }

        /// <summary>
        /// Dados de alugueres semanais para gráfico.
        /// Lista com número de alugueres de cada semana.
        /// </summary>
        public List<int> WeeklyRentalsData { get; set; }

        #endregion

        #region Stock de Máquinas

        /// <summary>
        /// Número total de máquinas em stock.
        /// </summary>
        [DisplayName("Total de Máquinas")]
        public int TotalMachines { get; set; }

        /// <summary>
        /// Número de máquinas disponíveis para venda ou aluguer.
        /// </summary>
        [DisplayName("Máquinas Disponíveis")]
        public int AvailableMachines { get; set; }

        /// <summary>
        /// Número de máquinas atualmente em manutenção.
        /// </summary>
        [DisplayName("Máquinas em Manutenção")]
        public int MachinesInMaintenance { get; set; }

        #endregion

        /// <summary>
        /// Construtor padrão. Inicializa as coleções vazias.
        /// </summary>
        public DashboardViewModel()
        {
            MonthlyRevenueData = new Dictionary<string, decimal>();
            WeeklyRentalsData = new List<int>();
        }
    }
}
