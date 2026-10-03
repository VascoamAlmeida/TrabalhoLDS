using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LojaMaquinas.WebApp.ViewModels
{
    /// <summary>
    /// ViewModel de cliente para views MVC.
    /// Representa dados completos de cliente com validações e utilizador associado.
    /// </summary>
    public class ClientViewModel
    {
        /// <summary>
        /// Identificador único do cliente.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Identificador do utilizador associado ao cliente (obrigatório).
        /// </summary>
        [DisplayName("User ID")]
        public Guid UserId { get; set; }

        /// <summary>
        /// Primeiro nome do cliente (obrigatório, 2-20 caracteres).
        /// </summary>
        [DisplayName("Primeiro Nome")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [StringLength(20, ErrorMessage = "O campo {0} deve ter entre {2} e {1} caracteres", MinimumLength = 2)]
        public string FirstName { get; set; }

        /// <summary>
        /// Último nome do cliente (obrigatório, 2-20 caracteres).
        /// </summary>
        [DisplayName("Último Nome")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [StringLength(20, ErrorMessage = "O campo {0} deve ter entre {2} e {1} caracteres", MinimumLength = 2)]
        public string LastName { get; set; }

        /// <summary>
        /// Morada do cliente (obrigatório, 5-50 caracteres).
        /// </summary>
        [DisplayName("Morada")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [StringLength(50, ErrorMessage = "O campo {0} deve ter entre {2} e {1} caracteres", MinimumLength = 5)]
        public string Address { get; set; }

        /// <summary>
        /// Código postal no formato XXXX-XXX (obrigatório).
        /// </summary>
        [DisplayName("Código Postal")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [RegularExpression(@"^\d{4}-\d{3}$", ErrorMessage = "O {0} deve estar no formato XXXX-XXX")]
        public string PostalCode { get; set; }

        /// <summary>
        /// Número de telefone com 9 dígitos (obrigatório).
        /// </summary>
        [DisplayName("Telemóvel")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "O {0} deve ter 9 dígitos")]
        public string PhoneNumber { get; set; }

        /// <summary>
        /// Número de Identificação Fiscal com 9 dígitos (obrigatório).
        /// </summary>
        [DisplayName("NIF")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        [RegularExpression(@"^\d{9}$", ErrorMessage = "O {0} deve ter 9 dígitos")]
        public string NIF { get; set; }

        /// <summary>
        /// Utilizador associado ao cliente (navegação).
        /// </summary>
        [ValidateNever]
        public UserViewModel User { get; set; }
    }
}
