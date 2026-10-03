using LojaMaquinas.Business.Models.Enums;

namespace LojaMaquinas.Business.Models.Entities
{
    /// <summary>
    /// Representa uma unidade individual de stock/inventário.
    /// Cada item de stock corresponde a uma unidade física de um produto.
    /// </summary>
    public class Stock : Entity
    {
        /// <summary>
        /// Referência única do item de stock.
        /// Utilizado para rastreabilidade individual de equipamentos.
        /// </summary>
        public Guid Ref { get; set; }

        /// <summary>
        /// Identificador do produto a que este stock pertence.
        /// Chave estrangeira para a tabela Product.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Estado atual do stock (Available, SoldOut, NotAvailable).
        /// Indica se o item está disponível para venda/aluguer.
        /// </summary>
        public StockStatus StockStatus { get; set; }

        /* EF Relations */

        /// <summary>
        /// Produto a que este item de stock pertence.
        /// Navigation property do Entity Framework.
        /// </summary>
        public Product Product { get; set; }

        /// <summary>
        /// Coleção de pedidos que incluem este item de stock.
        /// Navigation property do Entity Framework.
        /// </summary>
        public IEnumerable<Order> Orders { get; set; }

    }
}
