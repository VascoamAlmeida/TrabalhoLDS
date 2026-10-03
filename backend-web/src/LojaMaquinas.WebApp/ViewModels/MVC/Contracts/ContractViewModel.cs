using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.WebApp.ViewModels.MVC.OrderRent;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace LojaMaquinas.WebApp.ViewModels.MVC.Contracts
{
    /// <summary>
    /// ViewModel de contrato para views MVC.
    /// Representa um contrato de aluguer com termos e OrderRent associado.
    /// </summary>
    public class ContractViewModel
    {

		/// <summary>
		/// Id do contract
		/// </summary>
		[Key]
		[DisplayName("Id do Contract")]
		public Guid Id { get; set; }

		/// <summary>
		/// Identificador do aluguer (OrderRent) associado ao contrato.
		/// </summary>
		public Guid OrderRentId { get; set; }

		/// <summary>
		/// Apresenta o nome da maquina 
		/// </summary>
		[DisplayName("Equipamento")]
		public string MachineName { get; set; }

		/// <summary>
		/// Apresenta o modelo da maquina
		/// </summary>
		[DisplayName("Modelo")]
		public string MachineModel { get; set; }

		/// <summary>
		/// Apresenta a data inicial do contrato
		/// </summary>
		[DisplayName("Data de início")]
		public DateTime StartDate { get; set; }

		/// <summary>
		/// Apresenta a data final do contrato
		/// </summary>
		[DisplayName("Data de fim")]
		public DateTime EndDate { get; set; }

		/// <summary>
		/// Apresenta a duração do aluguer
		/// </summary>
		[DisplayName("Duração")]
		public int Duration { get; set; }

		/// <summary>
		/// Preço diario do emprestimo
		/// </summary>
		[DisplayName("Preço diário")]
		public decimal DailyPrice { get; set; }

		/// <summary>
		/// Preço total do emprestimo
		/// </summary>
		[DisplayName("Total")]
		public decimal Total {  get; set; }

		/// <summary>
		/// Termos e condições do contrato em formato texto.
		/// </summary>
		[DisplayName("Termos e Condições")] 
        public string TermsConditions { get; set; }

        /// <summary>
        /// Apresenta a opçao de aceitar os termos, pelo que tem de ser aceites
        /// </summary>
		[DisplayName("Li e aceito os Termos e Condições")]
		public bool AcceptTerms { get; set; }

		/// <summary>
		/// Apresenta nome do cliente
		/// </summary>
		[DisplayName("Nome")]
		public string ClientName { get; set; }

        /// <summary>
        /// Apresenta email do cliente
        /// </summary>
		[DisplayName("Email")]
		public string ClientEmail { get; set; }

        /// <summary>
        /// Apresenta numero telemovel do cliente
        /// </summary>
		[DisplayName("Telemóvel")]
		public string ClientPhone { get; set; }

        /// <summary>
        /// Apresenta morada do cliente
        /// </summary>
		[DisplayName("Morada")]
		public string ClientAddress { get; set; }

	}
}