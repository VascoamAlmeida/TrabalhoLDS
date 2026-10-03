using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace LojaMaquinas.WebApp.ViewModels.MVC
{
    /// <summary>
    /// ViewModel de categoria para views MVC.
    /// Representa dados de categoria com validações e produtos associados.
    /// </summary>
    public class CategoryViewModel
    {
        /// <summary>
        /// Identificador único da categoria.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        /// <summary>
        /// Nome da categoria (obrigatório, 3-30 caracteres).
        /// </summary>
        [DisplayName("Category Name")]
        [Required(ErrorMessage = "The field {0} is mandatory")]
        [StringLength(30, ErrorMessage = "The field {0} must be between {2} and {1} characters", MinimumLength = 3)]
        public string Name { get; set; }

        /// <summary>
        /// Descrição da categoria (obrigatório, 3-250 caracteres).
        /// </summary>
        [DisplayName("Category Description")]
        [Required(ErrorMessage = "The field {0} is mandatory")]
        [StringLength(250, ErrorMessage = "The field {0} must be between {2} and {1} characters", MinimumLength = 3)]
        public string Description { get; set; }

        /// <summary>
        /// Coleção de produtos associados a esta categoria.
        /// </summary>
        [ValidateNever]
        public IEnumerable<ProductViewModel> Products { get; set; }
    }
}
