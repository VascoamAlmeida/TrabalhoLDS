using LojaMaquinas.Business.Models.Enums;

namespace LojaMaquinas.Business.Models.Entities
{
    /// <summary>
    /// Representa uma manutenção agendada ou realizada num equipamento.
    /// Associada a um pedido (Order) para rastreabilidade de serviços.
    /// </summary>
    public class Maintenance : Entity
    {
        /// <summary>
        /// Identificador do pedido (Order) ao qual esta manutenção está associada.
        /// Chave estrangeira para a tabela Order.
        /// </summary>
        public Guid OrderId { get; set; }

        /// <summary>
        /// Data agendada para a manutenção.
        /// Pode ser uma data futura (manutenção agendada) ou passada (manutenção realizada).
        /// </summary>
        public DateTime ScheduledDate { get; set; }

        /// <summary>
        /// Preço cobrado pela manutenção.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Observações sobre a manutenção.
        /// Contém detalhes técnicos, problemas encontrados ou trabalhos realizados.
        /// </summary>
        public string Obs { get; set; }

        /// <summary>
        /// Estado atual da manutenção (Scheduled, InProgress, Completed, Cancelled).
        /// </summary>
        public MaintenanceStatus Status { get; set; }

        /* EF Relations */

        /// <summary>
        /// Pedido (Order) ao qual esta manutenção está associada.
        /// Navigation property do Entity Framework.
        /// </summary>
        public Order Order { get; set; }
    }
}
