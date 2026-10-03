using FluentValidation;
using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.Business.Validations.Entities
{
    /// <summary>
    /// Validador para a entidade OrderRent usando FluentValidation.
    /// Define regras de validação para alugueres, incluindo validação de datas.
    /// </summary>
    public class OrderRentValidation : AbstractValidator<OrderRent>
    {
        /// <summary>
        /// Inicializa as regras de validação para OrderRent.
        /// Valida OrderId, data inicial e final (EndDate deve ser posterior a InitialDate).
        /// </summary>
        public OrderRentValidation()
        {
            RuleFor(or => or.OrderId).NotNull().WithMessage("The {PropertyName} is mandatory!")
             .NotEmpty().WithMessage("The {PropertyName} must not be empty!");

            RuleFor(or => or.InitialDate)
            .NotEmpty().WithMessage("The {PropertyName} is mandatory.");

            RuleFor(or => or.EndDate)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory.")
                .GreaterThan(or => or.InitialDate)
                .WithMessage("{PropertyName} must be after the Initial Date.");

        }
    }
}
