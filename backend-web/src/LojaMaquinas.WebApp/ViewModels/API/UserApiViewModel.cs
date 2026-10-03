using System.Text.Json.Serialization;

namespace LojaMaquinas.WebApp.ViewModels.API
{
    /// <summary>
    /// ViewModel de utilizador para API REST.
    /// Representa os dados públicos do utilizador e os campos aceites nas operações de escrita.
    /// </summary>
    public class UserApiViewModel
    {
        /// <summary>
        /// Identificador único do utilizador.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Email do utilizador (usado para login).
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Password recebida apenas em operações de escrita. Nunca é devolvida pela API.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Password { get; set; }

        /// <summary>
        /// Tipo de utilizador (Admin, Client, etc.).
        /// </summary>
        public string UserType { get; set; }

        /// <summary>
        /// Indica se o utilizador está ativo no sistema.
        /// </summary>
        public bool IsActive { get; set; }
    }
}
