using FluentValidation;
using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.Business.Validations.Entities
{
    /// <summary>
    /// Validador para a entidade Maintenance usando FluentValidation.
    /// Define regras de validação para agendamento e gestão de manutenções.
    /// </summary>
    public class MaintenanceValidation : AbstractValidator<Maintenance>
    {
        /// <summary>
        /// Inicializa as regras de validação para Maintenance.
        /// Valida OrderId, data de agendamento (deve ser futura), preço, observações e estado.
        /// </summary>
        public MaintenanceValidation()
        {
            RuleFor(m => m.OrderId)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!");

            RuleFor(m => m.ScheduledDate)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .GreaterThan(DateTime.Now).WithMessage("The {PropertyName} must be in the future");

            RuleFor(m => m.Price)
                .GreaterThanOrEqualTo(0).WithMessage("The {PropertyName} must be zero or greater");

            RuleFor(m => m.Obs)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .Length(5, 500).WithMessage("The {PropertyName} must be between {MinLength} and {MaxLength} characters");

            RuleFor(m => m.Status)
                .IsInEnum().WithMessage("The {PropertyName} must be a valid maintenance status");
        }
    }
}
