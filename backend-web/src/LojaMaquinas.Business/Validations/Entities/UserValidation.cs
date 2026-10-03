
using FluentValidation;
using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.Business.Validations.Entities
    {
    /// <summary>
    /// Validador para a entidade User usando FluentValidation.
    /// Define regras de validação para credenciais e tipo de utilizador.
    /// </summary>
    public class UserValidation : AbstractValidator<User>
        {
        /// <summary>
        /// Inicializa as regras de validação para User.
        /// Valida email, password e tipo de utilizador (ADMIN ou CLIENT).
        /// </summary>
        public UserValidation()
            {
            RuleFor(u => u.Email)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .EmailAddress().WithMessage("The {PropertyName} must be a valid email address")
                .Length(5, 50).WithMessage("The {PropertyName} must be between {MinLength} and {MaxLength} characters");

            RuleFor(u => u.Password)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .MinimumLength(6).WithMessage("The {PropertyName} must have at least {MinLength} characters");

            RuleFor(u => u.UserType)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .Must(type => type == "ADMIN" || type == "CLIENT")
                .WithMessage("The {PropertyName} must be either 'ADMIN' or 'CLIENT'");
            }
        }
    }
