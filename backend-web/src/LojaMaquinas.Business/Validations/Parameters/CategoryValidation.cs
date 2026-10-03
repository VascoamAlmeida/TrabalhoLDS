using FluentValidation;
using LojaMaquinas.Business.Models.Parameters;

namespace LojaMaquinas.Business.Validations.Parameters
{
    /// <summary>
    /// Validador para a entidade Category usando FluentValidation.
    /// Define regras de validação para categorias de produtos.
    /// </summary>
    public class CategoryValidation : AbstractValidator<Category>
    {
        /// <summary>
        /// Inicializa as regras de validação para Category.
        /// Valida nome e descrição da categoria.
        /// </summary>
        public CategoryValidation()
        {
            RuleFor(c => c.Name).NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .Length(3, 30).WithMessage("The {PropertyName} must be between {MinLength} and {MaxLength} characters");

            RuleFor(c => c.Description).NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .Length(3,250).WithMessage("The {PropertyName} must be between {MinLength} and {MaxLength} characters");
        }
    }
}
