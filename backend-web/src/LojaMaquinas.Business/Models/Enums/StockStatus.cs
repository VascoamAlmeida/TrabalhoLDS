using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LojaMaquinas.Business.Models.Enums
{
    /// <summary>
    /// Define os estados possíveis de um item de stock.
    /// </summary>
    public enum StockStatus
    {
        /// <summary>
        /// Item disponível para venda ou aluguer.
        /// </summary>
        Available = 0,

        /// <summary>
        /// Item vendido e sem stock disponível.
        /// </summary>
        SoldOut = 1,

        /// <summary>
        /// Item não disponível (em manutenção, alugado ou reservado).
        /// </summary>
        NotAvailable
    }
}
