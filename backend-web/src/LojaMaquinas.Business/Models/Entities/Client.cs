namespace LojaMaquinas.Business.Models.Entities
    {
    /// <summary>
    /// Representa um cliente do sistema de loja de máquinas.
    /// Contém informações pessoais e fiscais do cliente.
    /// </summary>
    public class Client : Entity
        {
        /// <summary>
        /// Identificador do utilizador associado ao cliente.
        /// Chave estrangeira para a tabela User (relação 1:1).
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Primeiro nome do cliente.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Apelido do cliente.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Endereço de morada do cliente.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Código postal do cliente.
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Número de telefone de contacto do cliente.
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Número de Identificação Fiscal (NIF) do cliente.
        /// Deve ser único no sistema.
        /// </summary>
        public string NIF { get; set; }

        /* EF Relations */

        /// <summary>
        /// Utilizador associado a este cliente (relação 1:1).
        /// Navigation property do Entity Framework.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// Coleção de pedidos realizados por este cliente.
        /// Navigation property do Entity Framework.
        /// </summary>
        public IEnumerable<Order> Orders { get; set; }
        }
    }
