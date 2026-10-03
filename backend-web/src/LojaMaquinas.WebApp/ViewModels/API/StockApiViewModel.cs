using LojaMaquinas.Business.Models.Enums;

namespace LojaMaquinas.WebApp.ViewModels.MVC
{
    /// <summary>
    /// ViewModel de stock para API REST.
    /// Representa um item individual de inventário com referência e estado.
    /// </summary>
    public class StockApiViewModel
    {
        /// <summary>
        /// Identificador único do item de stock.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Referência única do item de stock (usado para tracking).
        /// </summary>
        public Guid Ref { get; set; }

        /// <summary>
        /// Identificador do produto associado a este item de stock.
        /// </summary>
        public Guid ProductId { get; set; }

        /// <summary>
        /// Estado do stock (Available=1, SoldOut=2, NotAvailable=3).
        /// </summary>
        public StockStatus StockStatus { get; set; }
    }
}