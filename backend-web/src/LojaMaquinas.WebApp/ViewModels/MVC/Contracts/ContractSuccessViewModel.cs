using System.ComponentModel;

namespace LojaMaquinas.WebApp.ViewModels.MVC.Contracts
{
    /// <summary>
    /// ViewModel do sucesso de contrato para views MVC.
    /// </summary>
    public class ContractSuccessViewModel
	{
		/// <summary>
		/// Nome do equipamento que foi alugado
		/// </summary>
		[DisplayName("Nome do Equipamento")]
		public string EquipmentName { get; set; }

		/// <summary>
		/// Total a pagar pelo contrato
		/// </summary>
		[DisplayName("Total")]
		public decimal Total { get; set; }

		/// <summary>
		/// Tipo de transação que foi realizado
		/// </summary>
		[DisplayName("Tipo de Transação")]
		public string TransactionType { get; set; } = "Aluguer";

		/// <summary>
		/// Numero referencia associado ao contrato
		/// </summary>
		[DisplayName("Número de Referência")]
		public string ReferenceNumber { get; set; }
	}
}

