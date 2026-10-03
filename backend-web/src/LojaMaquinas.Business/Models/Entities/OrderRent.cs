using LojaMaquinas.Business.Models.Enums;

namespace LojaMaquinas.Business.Models.Entities
{
    /// <summary>
    /// Representa informações específicas de um aluguer de equipamento.
    /// Complementa o pedido (Order) com dados de datas e contratos.
    /// </summary>
    public class OrderRent : Entity
    {
        /// <summary>
        /// Identificador do pedido (Order) associado a este aluguer.
        /// Chave estrangeira para a tabela Order (relação 1:1).
        /// </summary>
        public Guid OrderId { get; set; }

        /// <summary>
        /// Data de início do período de aluguer.
        /// </summary>
        public DateTime InitialDate { get; set; }

        /// <summary>
        /// Data de fim do período de aluguer.
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Assinatura ou imagem da empresa no contrato.
        /// Utilizada para validação do contrato de aluguer.
        /// </summary>
        public string CompanyImage { get; set; }

        /// <summary>
        /// Assinatura ou imagem do utilizador no contrato.
        /// Opcional - utilizada quando o cliente assina digitalmente.
        /// </summary>
        public string? UserImage { get; set; }

        /// <summary>
        /// Estado atual do aluguer (Active, Completed, Cancelled, Expired).
        /// </summary>
        public OrderRentStatus Status { get; set; }

        /* EF Relations */

        /// <summary>
        /// Contrato associado a este aluguer.
        /// Nullable - gerado após confirmação do pedido.
        /// Navigation property do Entity Framework.
        /// </summary>
        public Contract? Contract { get; set; }

        /// <summary>
        /// Pedido (Order) associado a este aluguer.
        /// Navigation property do Entity Framework.
        /// </summary>
        public Order Order { get; set; }
    }
}
