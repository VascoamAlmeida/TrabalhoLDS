using LojaMaquinas.Business.Models.Enums;

namespace LojaMaquinas.Business.Models.Entities
{
    /// <summary>
    /// Representa um pedido de compra ou aluguer realizado por um cliente.
    /// Contém informações sobre o tipo de transação, valores e estados.
    /// </summary>
    public class Order : Entity
    {
        /// <summary>
        /// Identificador do cliente que realizou o pedido.
        /// Chave estrangeira para a tabela Client.
        /// </summary>
        public Guid ClientId { get; set; }

        /// <summary>
        /// Identificador do item de stock associado ao pedido.
        /// Chave estrangeira para a tabela Stock.
        /// </summary>
        public Guid StockId {  get; set; }

        /// <summary>
        /// Data e hora em que o pedido foi realizado.
        /// </summary>
        public DateTime OrderDate {  get; set; }

        /// <summary>
        /// Valor total do pedido.
        /// Para aluguer, representa o valor total calculado com base na duração.
        /// </summary>
        public decimal Total {  get; set; }

        /// <summary>
        /// Tipo de pedido (Sale ou Rent).
        /// Define se é uma compra ou aluguer.
        /// </summary>
        public OrderType Type { get; set; }

        /// <summary>
        /// Estado atual do pedido (Pending, Confirmed, Cancelled, Completed).
        /// </summary>
        public OrderStatus Status { get; set; }

        /* EF Relations */

        /// <summary>
        /// Informações adicionais de aluguer, se o pedido for do tipo Rent.
        /// Nullable - apenas preenchido para pedidos de aluguer.
        /// Navigation property do Entity Framework.
        /// </summary>
        public OrderRent? OrderRent { get; set; }

        /// <summary>
        /// Cliente que realizou o pedido.
        /// Navigation property do Entity Framework.
        /// </summary>
        public Client Client { get; set; }

        /// <summary>
        /// Item de stock associado ao pedido.
        /// Navigation property do Entity Framework.
        /// </summary>
        public Stock Stock { get; set; }

        /// <summary>
        /// Coleção de manutenções associadas a este pedido.
        /// Navigation property do Entity Framework.
        /// </summary>
        public IEnumerable<Maintenance> Maintenances { get; set; }
    }
}