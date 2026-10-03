namespace LojaMaquinas.Business.Models.Entities
    {
    /// <summary>
    /// Representa um utilizador do sistema com credenciais de autenticação.
    /// Contém informações de login e permissões de acesso.
    /// </summary>
    public class User : Entity
        {
        /// <summary>
        /// Endereço de email do utilizador.
        /// Utilizado como identificador único para autenticação.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Password encriptada do utilizador.
        /// Armazenada com hash para segurança.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Tipo de utilizador (ex: Admin, Cliente).
        /// Define as permissões de acesso ao sistema.
        /// </summary>
        public string UserType { get; set; }

        /// <summary>
        /// Indica se o utilizador está ativo no sistema.
        /// Utilizadores inativos não podem autenticar-se.
        /// </summary>
        public bool IsActive { get; set; }

        /* EF Relations */

        /// <summary>
        /// Cliente associado a este utilizador (relação 1:1).
        /// Navigation property do Entity Framework.
        /// </summary>
        public Client Client { get; set; }
        }
    }
