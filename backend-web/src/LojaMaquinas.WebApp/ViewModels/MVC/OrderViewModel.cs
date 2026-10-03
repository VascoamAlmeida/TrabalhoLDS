using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LojaMaquinas.WebApp.ViewModels.MVC
{
    /// <summary>
    /// ViewModel de pedido para views MVC.
    /// Representa um pedido de compra ou aluguer com estado e total.
    /// </summary>
    public class OrderViewModel
    {
        /// <summary>
        /// Identificador único do pedido.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador do cliente que realizou o pedido.
        /// </summary>
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [DisplayName("Cliente")]
        public Guid ClientId { get; set; }

        /// <summary>
        /// Identificador do item de stock associado ao pedido.
        /// </summary>
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [DisplayName("Produto")]
        public Guid StockId { get; set; }

        /// <summary>
        /// Data e hora em que o pedido foi criado.
        /// </summary>
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [DisplayName("Data")]
        public DateTime OrderDate { get; set; }

        /// <summary>
        /// Valor total do pedido.
        /// </summary>
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [DisplayName("Total")]
        public decimal Total { get; set; }

        /// <summary>
        /// Tipo de pedido (Sale=1, Rent=2).
        /// </summary>
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [DisplayName("Tipo de Pedido")]
        public OrderType Type { get; set; }

        /// <summary>
        /// Estado do pedido (Pending=1, Completed=2, Cancelled=3).
        /// </summary>
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [DisplayName("Estado do Pedido")]
        public OrderStatus Status { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [DisplayName("Produto")]
        [ValidateNever]
        public Stock Stock { get; set; }

        [ValidateNever]
        public Client Client { get; set; }

        [ValidateNever]
        public IEnumerable<Maintenance> Maintenances { get; set; }
    }
}
