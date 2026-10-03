using FluentValidation;
using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.Business.Validations.Entities
{
    /// <summary>
    /// Validador para a entidade Order usando FluentValidation.
    /// Define regras de validação para pedidos de compra e aluguer.
    /// </summary>
    public class OrderValidation : AbstractValidator<Order>
    {
        /// <summary>
        /// Inicializa as regras de validação para Order.
        /// Valida que o total do pedido seja maior que zero.
        /// </summary>
        public OrderValidation()
        {
            RuleFor(o => o.Total)
                .GreaterThan(0).WithMessage("The field {PropertyName} must be greater {ComparisonValue}");
        }
    }
}
