using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LojaMaquinas.WebApp.ViewModels
{
    /// <summary>
    /// ViewModel de utilizador para views MVC.
    /// Representa credenciais e tipo de utilizador com cliente associado.
    /// </summary>
    public class UserViewModel
    {
        /// <summary>
        /// Identificador único do utilizador.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Endereço de email do utilizador (obrigatório, 5-50 caracteres, formato válido).
        /// </summary>
        [DisplayName("Email")]
        [Required(ErrorMessage = "The field {0} is mandatory")]
        [EmailAddress(ErrorMessage = "The field {0} must be a valid email address")]
        [StringLength(50, ErrorMessage = "The field {0} must be between {2} and {1} characters", MinimumLength = 5)]
        public string Email { get; set; }

        /// <summary>
        /// Password do utilizador (obrigatório, mínimo 6 caracteres, máximo 250).
        /// </summary>
        [DisplayName("Password")]
        [Required(ErrorMessage = "The field {0} is mandatory")]
        [StringLength(250, ErrorMessage = "The field {0} must have at least {2} characters", MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        /// <summary>
        /// Tipo de utilizador (ex: "Admin", "Client" - obrigatório).
        /// </summary>
        [DisplayName("User Type")]
        [Required(ErrorMessage = "The field {0} is mandatory")]
        public string UserType { get; set; }

        /// <summary>
        /// Indica se o utilizador está ativo no sistema.
        /// </summary>
        [DisplayName("Is Active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// Cliente associado ao utilizador (navegação).
        /// </summary>
        [ValidateNever]
        public ClientViewModel Client { get; set; }
    }
}
