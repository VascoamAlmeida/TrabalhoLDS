using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace LojaMaquinas.WebApp.ViewModels.API
{
    /// <summary>
    /// ViewModel de aluguer (OrderRent) para API REST.
    /// Representa um pedido de aluguer com datas, imagens e estado.
    /// </summary>
    public class OrderRentApiViewModel
    {
        /// <summary>
        /// Identificador único do aluguer.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador do pedido (Order) associado ao aluguer.
        /// </summary>
        [Required]
        public Guid OrderId { get; set; }

        /// <summary>
        /// Data de início do período de aluguer.
        /// </summary>
        [Required]
        public DateTime InitialDate { get; set; }

        /// <summary>
        /// Data de fim do período de aluguer.
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// URL ou caminho da imagem da empresa (usado para documentação).
        /// </summary>
        public string CompanyImage { get; set; }

        /// <summary>
        /// URL ou caminho da imagem do utilizador (opcional, usado para documentação).
        /// </summary>
        public string? UserImage { get; set; }

        /// <summary>
        /// Estado do aluguer (Active=1, Completed=2, Cancelled=3).
        /// </summary>
        [Required]
        public OrderRentStatus Status { get; set; }

    }
}
