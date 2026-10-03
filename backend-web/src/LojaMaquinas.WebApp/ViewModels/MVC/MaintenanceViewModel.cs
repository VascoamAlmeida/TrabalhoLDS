using LojaMaquinas.Business.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LojaMaquinas.WebApp.ViewModels.MVC
{
    /// <summary>
    /// ViewModel de manutenção para views MVC.
    /// Representa dados completos de manutenção com validações.
    /// </summary>
    public class MaintenanceViewModel
    {
        /// <summary>
        /// Identificador único da manutenção.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador do pedido associado (obrigatório).
        /// </summary>
        [DisplayName("Pedido")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public Guid OrderId { get; set; }

        /// <summary>
        /// Data agendada para a manutenção (obrigatória).
        /// </summary>
        [DisplayName("Data Agendada")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [DataType(DataType.DateTime)]
        public DateTime ScheduledDate { get; set; }

        /// <summary>
        /// Preço da manutenção (obrigatório, deve ser maior ou igual a 0).
        /// </summary>
        [DisplayName("Preço")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [Range(0, double.MaxValue, ErrorMessage = "O campo {0} deve ser maior ou igual a 0")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        /// <summary>
        /// Observações sobre a manutenção (5-500 caracteres).
        /// </summary>
        [DisplayName("Observações")]
        [StringLength(500, ErrorMessage = "O campo {0} deve ter entre {2} e {1} caracteres", MinimumLength = 5)]
        public string Obs { get; set; }

        /// <summary>
        /// Estado atual da manutenção.
        /// </summary>
        [DisplayName("Estado")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public MaintenanceStatus Status { get; set; }

        /// <summary>
        /// Pedido associado à manutenção (navegação).
        /// </summary>
        [ValidateNever]
        public OrderViewModel Order { get; set; }
    }
}
