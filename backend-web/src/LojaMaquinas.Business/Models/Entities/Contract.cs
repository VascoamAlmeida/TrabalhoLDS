namespace LojaMaquinas.Business.Models.Entities
{
    /// <summary>
    /// Representa um contrato legal de aluguer de equipamento.
    /// Contém os termos e condições acordados entre a empresa e o cliente.
    /// </summary>
    public class Contract : Entity
    {
        /// <summary>
        /// Identificador do aluguer (OrderRent) associado a este contrato.
        /// Chave estrangeira para a tabela OrderRent (relação 1:1).
        /// </summary>
        public Guid OrderRentId { get; set; }

        /// <summary>
        /// Termos e condições do contrato.
        /// Texto completo com as cláusulas do acordo de aluguer.
        /// </summary>
        public string TermsConditions { get; set; }

        /* EF Relations */

        /// <summary>
        /// Aluguer (OrderRent) associado a este contrato.
        /// Navigation property do Entity Framework.
        /// </summary>
        public OrderRent? OrderRent { get; set; }
    }
}
