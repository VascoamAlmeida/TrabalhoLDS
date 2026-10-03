using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using LojaMaquinas.Business.Models.Enums;
using System;
using LojaMaquinas.WebApp.ViewModels.API;

namespace LojaMaquinas.WebApp.ViewModels.MVC
{
    /// <summary>
    /// ViewModel de stock para views MVC.
    /// Representa um item de stock com produto associado e histórico de pedidos.
    /// </summary>
    public class StockViewModel
    {
        // --- Identification ---
        /// <summary>
        /// Identificador único do item de stock.
        /// </summary>
        [Key]
        [DisplayName("Stock ID")]
        public Guid Id { get; set; }

        /// <summary>
        /// Referência única do item de stock (obrigatório).
        /// </summary>
        [Required(ErrorMessage = "The field {0} is mandatory")]
        [DisplayName("Stock Reference")]
        public Guid Ref { get; set; }

        // --- Foreign Key ---
        /// <summary>
        /// Identificador do produto associado (obrigatório).
        /// </summary>
        [Required(ErrorMessage = "The field {0} is mandatory")]
        [DisplayName("Product")]
        public Guid ProductId { get; set; }

        // --- Status ---
        /// <summary>
        /// Estado do stock (Available=1, SoldOut=2, NotAvailable=3 - obrigatório).
        /// </summary>
        [Required(ErrorMessage = "The field {0} is mandatory")]
        [DisplayName("Status")]
        public StockStatus StockStatus { get; set; }

        // --- Related Data ---

        /// <summary>
        /// Produto associado ao item de stock (navegação).
        /// </summary>
        [ValidateNever]
        public ProductViewModel Product { get; set; }

        /// <summary>
        /// Histórico de pedidos associados a este item de stock (navegação).
        /// </summary>
        [ValidateNever]
        [DisplayName("Order History")]
        public IEnumerable<OrderApiViewModel>? Orders { get; set; }
    }
}