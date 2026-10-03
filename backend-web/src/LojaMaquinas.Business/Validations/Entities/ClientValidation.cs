using FluentValidation;
using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.Business.Validations.Entities
{
    /// <summary>
    /// Validador para a entidade Client usando FluentValidation.
    /// Define regras de validação para dados pessoais e fiscais do cliente.
    /// </summary>
    public class ClientValidation : AbstractValidator<Client>
    {
        /// <summary>
        /// Inicializa as regras de validação para Client.
        /// Valida UserId, nomes, morada, código postal, telefone e NIF.
        /// </summary>
        public ClientValidation()
        {
            RuleFor(c => c.UserId)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!");

            RuleFor(c => c.FirstName)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .Length(2, 20).WithMessage("The {PropertyName} must be between {MinLength} and {MaxLength} characters");

            RuleFor(c => c.LastName)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .Length(2, 20).WithMessage("The {PropertyName} must be between {MinLength} and {MaxLength} characters");

            RuleFor(c => c.Address)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .Length(5, 50).WithMessage("The {PropertyName} must be between {MinLength} and {MaxLength} characters");

            RuleFor(c => c.PostalCode)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .Matches(@"^\d{4}-\d{3}$").WithMessage("The {PropertyName} must be in format XXXX-XXX");

            RuleFor(c => c.PhoneNumber)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .Matches(@"^\d{9}$").WithMessage("The {PropertyName} must have 9 digits");

            RuleFor(c => c.NIF)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .Matches(@"^\d{9}$").WithMessage("The {PropertyName} must have 9 digits");
        }
    }
}
