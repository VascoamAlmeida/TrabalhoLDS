using LojaMaquinas.Business.Models.Enums;

namespace LojaMaquinas.WebApp.ViewModels.API
{
    /// <summary>
    /// ViewModel de pedido para API REST.
    /// Representa um pedido de compra ou aluguer com estado e total.
    /// </summary>
    public class OrderApiViewModel
    {
        /// <summary>
        /// Identificador único do pedido.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador do cliente que realizou o pedido.
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        /// Identificador do item de stock associado ao pedido.
        /// </summary>
        public Guid StockId { get; set; }

        /// <summary>
        /// Data e hora em que o pedido foi criado.
        /// </summary>
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Valor total do pedido.
        /// </summary>
        public decimal Total { get; set; }

        /// <summary>
        /// Tipo de pedido (Sale=1, Rent=2).
        /// </summary>
        public OrderType Type { get; set; }

        /// <summary>
        /// Estado do pedido (Pending=1, Completed=2, Cancelled=3).
        /// </summary>
        public OrderStatus Status { get; set; }
    }
}
