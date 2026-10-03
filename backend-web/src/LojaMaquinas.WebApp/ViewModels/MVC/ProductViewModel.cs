using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using LojaMaquinas.Business.Models.Enums;

namespace LojaMaquinas.WebApp.ViewModels.MVC
{
    /// <summary>
    /// ViewModel de produto para views MVC.
    /// Representa dados completos de produto com validações, categoria e stock associado.
    /// </summary>
    public class ProductViewModel
    {
        /// <summary>
        /// Identificador único do produto.
        /// </summary>
        [Key]
        public Guid Id { get; set; }

        // --- Basic Product Properties ---
        /// <summary>
        /// Nome do produto (obrigatório, 3-50 caracteres).
        /// </summary>
        [Required(ErrorMessage = "The field {0} is mandatory")]
        [DisplayName("Name")]
        [StringLength(50, ErrorMessage = "The field {0} must be between {2} and {1} characters", MinimumLength = 3)]
        public string Name { get; set; }

        /// <summary>
        /// Tipo de produto (Compra=1, Aluguer=2 - obrigatório).
        /// </summary>
        [Required(ErrorMessage = "The field {0} is mandatory")]
        [DisplayName("Type")]
        public ProductType Type { get; set; }

        /// <summary>
        /// Marca do produto (obrigatório, 2-30 caracteres).
        /// </summary>
        [Required(ErrorMessage = "The field {0} is mandatory")]
        [DisplayName("Brand")]
        [StringLength(30, ErrorMessage = "The field {0} must be between {2} and {1} characters", MinimumLength = 2)]
        public string Brand { get; set; }

        /// <summary>
        /// Modelo do produto (obrigatório, 2-50 caracteres).
        /// </summary>
        [Required(ErrorMessage = "The field {0} is mandatory")]
        [DisplayName("Model")]
        [StringLength(50, ErrorMessage = "The field {0} must be between {2} and {1} characters", MinimumLength = 2)]
        public string Model { get; set; }

        /// <summary>
        /// Ano de fabrico do produto (obrigatório).
        /// </summary>
        [Required(ErrorMessage = "The field {0} is mandatory")]
        [DisplayName("Year of Manufacture")]
        public int YearManufacture { get; set; }

        /// <summary>
        /// Descrição detalhada do produto (obrigatório, 10-500 caracteres).
        /// </summary>
        [Required(ErrorMessage = "The field {0} is mandatory")]
        [DisplayName("Description")]
        [StringLength(500, ErrorMessage = "The field {0} must be between {2} and {1} characters", MinimumLength = 10)]
        public string Description { get; set; }

        /// <summary>
        /// Preço do produto (obrigatório, formato moeda).
        /// </summary>
        [Required(ErrorMessage = "The field {0} is mandatory")]
        [DisplayName("Price")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        /// <summary>
        /// Quantidade inicial em stock (obrigatório).
        /// </summary>
        [Required(ErrorMessage = "The field {0} is mandatory")]
        [DisplayName("Initial Quantity")]
        public int Quantity { get; set; }

        /// <summary>
        /// URL ou caminho da imagem do produto.
        /// </summary>
        [DisplayName("Image URL")]
        public string Image { get; set; }

        /// <summary>
        /// Indica se o produto está ativo no sistema.
        /// </summary>
        [DisplayName("Active")]
        public bool IsActive { get; set; }


        // --- Foreign Key ---
        /// <summary>
        /// Identificador da categoria à qual o produto pertence (obrigatório).
        /// </summary>
        [Required(ErrorMessage = "The field {0} is mandatory")]
        [DisplayName("Category")]
        public Guid CategoryId { get; set; }


        // --- Related Data ---
        /// <summary>
        /// Categoria associada ao produto (navegação).
        /// </summary>
        [ValidateNever]
        public CategoryViewModel Category { get; set; }

        /// <summary>
        /// Coleção de items de stock atuais do produto (navegação).
        /// </summary>
        [ValidateNever]
        [DisplayName("Current Stock Items")]
        public IEnumerable<StockViewModel>? Stocks { get; set; }
    }
}