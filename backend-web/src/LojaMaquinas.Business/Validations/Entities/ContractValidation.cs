using FluentValidation;
using LojaMaquinas.Business.Models.Entities;


namespace LojaMaquinas.Business.Validations.Entities
{
    /// <summary>
    /// Validador para a entidade Contract usando FluentValidation.
    /// Define regras de validação para contratos de aluguer.
    /// </summary>
    internal class ContractValidation : AbstractValidator<Contract>
    {
        /// <summary>
        /// Inicializa as regras de validação para Contract.
        /// Valida OrderRentId e termos e condições do contrato.
        /// </summary>
        public ContractValidation()
        {
            RuleFor(c => c.OrderRentId).NotNull().WithMessage("The {PropertyName} is mandatory!")
                .NotEmpty().WithMessage("The {PropertyName} must not be empty!");

            RuleFor(c => c.TermsConditions).NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .Length(3,10000).WithMessage("The {PropertyName} must be between {MinLength} and {MaxLength} characters");

        }
    }
}
