using LojaMaquinas.Business.Models.Enums;

namespace LojaMaquinas.WebApp.ViewModels.API
{
    /// <summary>
    /// ViewModel de manutenção para API REST.
    /// Representa uma manutenção agendada com data, preço e estado.
    /// </summary>
    public class MaintenanceApiViewModel
    {
        /// <summary>
        /// Identificador único da manutenção.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador do pedido associado à manutenção.
        /// </summary>
        public Guid OrderId { get; set; }

        /// <summary>
        /// Data e hora agendada para a manutenção.
        /// </summary>
        public DateTime ScheduledDate { get; set; }

        /// <summary>
        /// Preço da manutenção.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Observações sobre a manutenção.
        /// </summary>
        public string Obs { get; set; }

        /// <summary>
        /// Estado da manutenção (Pending=1, Completed=2, Cancelled=3).
        /// </summary>
        public MaintenanceStatus Status { get; set; }
    }
}
