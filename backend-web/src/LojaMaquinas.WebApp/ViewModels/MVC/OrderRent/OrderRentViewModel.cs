using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;

namespace LojaMaquinas.WebApp.ViewModels.MVC.OrderRent
{
    /// <summary>
    /// ViewModel de aluguer (OrderRent) para views MVC.
    /// Representa um pedido de aluguer com datas, imagens, estado e relações com contrato e pedido.
    /// </summary>
    public class OrderRentViewModel
    {
        /// <summary>
        /// Identificador único do aluguer.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador do pedido (Order) associado (obrigatório).
        /// </summary>
        [DisplayName("Order")]
        [Required(ErrorMessage = "The field {0} is mandatory")]
        public Guid OrderId { get; set; }

        /// <summary>
        /// Data de início do período de aluguer (obrigatório).
        /// </summary>
        [Required]
        public DateTime InitialDate { get; set; }

        /// <summary>
        /// Data de fim do período de aluguer (obrigatório).
        /// </summary>
        [Required]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// URL ou caminho da imagem da empresa.
        /// </summary>
        public string CompanyImage { get; set; }

        /// <summary>
        /// URL ou caminho da imagem do utilizador (opcional).
        /// </summary>
        public string? UserImage { get; set; }

        /// <summary>
        /// Estado do aluguer (Active=1, Completed=2, Cancelled=3 - obrigatório).
        /// </summary>
        [Required]
        public OrderRentStatus Status { get; set; }

        /// <summary>
        /// Contrato associado ao aluguer (navegação).
        /// </summary>
        public Contract? Contract { get; set; }

        /// <summary>
        /// Pedido associado ao aluguer (navegação).
        /// </summary>
        public Order? Order { get; set; }
    }
}
