using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.WebApp.ViewModels.API
{
    /// <summary>
    /// ViewModel de contrato para API REST.
    /// Representa um contrato de aluguer com termos e condições.
    /// </summary>
    public class ContractApiViewModel
    {
        /// <summary>
        /// Identificador único do contrato.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador do aluguer (OrderRent) associado ao contrato.
        /// </summary>
        public Guid OrderRentId { get; set; }

        /// <summary>
        /// Termos e condições do contrato em formato texto.
        /// </summary>
        public string TermsConditions { get; set; }

    }
}
