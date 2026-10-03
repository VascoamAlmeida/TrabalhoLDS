namespace LojaMaquinas.WebApp.ViewModels.API
{
    /// <summary>
    /// ViewModel de cliente para API REST.
    /// Representa dados de cliente incluindo informações pessoais e fiscais.
    /// </summary>
    public class ClientApiViewModel
    {
        /// <summary>
        /// Identificador único do cliente.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador do utilizador associado ao cliente.
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// Primeiro nome do cliente.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Último nome (apelido) do cliente.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Morada completa do cliente.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// Código postal no formato XXXX-XXX.
        /// </summary>
        public string PostalCode { get; set; }

        /// <summary>
        /// Número de telefone do cliente (9 dígitos).
        /// </summary>
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Número de Identificação Fiscal português (9 dígitos).
        /// </summary>
        public string NIF { get; set; }
    }
}
