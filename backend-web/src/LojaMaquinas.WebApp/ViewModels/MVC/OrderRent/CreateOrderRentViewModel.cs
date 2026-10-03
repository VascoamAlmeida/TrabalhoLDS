using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LojaMaquinas.WebApp.ViewModels.MVC.OrderRent
{
    /// <summary>
    /// View de criar a orderRent
    /// </summary>
    public class CreateOrderRentViewModel
    {

		/// <summary>
		/// Order associado ao aluguer.
		/// </summary>
		public Guid OrderId { get; set; }

		/// <summary>
		/// Maquina que esta ser alugada
		/// </summary>
		public Guid MachineId { get; set; }

        /// <summary>
        /// Nome da maquina
        /// </summary>
        [DisplayName("Maquina que está a ser alugada")]
        public string? MachineName { get; set; }

        /// <summary>
        /// Data que começa aluguer
        /// </summary>
        [DisplayName("Data inicial de aluguer")]
        [Required(ErrorMessage = "Tem de existir data inicial de aluguer")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Data que acaba aluguer
        /// </summary>
        [DisplayName("Data final de aluguer")]
        [Required(ErrorMessage = "Tem de existir data final de aluguer")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Foto do estado da maquina de como sai da empresa
        /// </summary>
        [DisplayName("Foto adicionada da empresa")]
        [Required(ErrorMessage = "Necessário inserir foto de empresa")]
        public IFormFile CompanyPhoto { get; set; }

        /// <summary>
        /// Preço a ser pago
        /// </summary>
        [DisplayName("Total to pay")]
        public decimal Total { get; set; }

    }
}
