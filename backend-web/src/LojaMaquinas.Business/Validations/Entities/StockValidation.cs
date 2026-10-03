using FluentValidation;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using System;

namespace LojaMaquinas.Business.Validations.Entities
{
    /// <summary>
    /// Validador para a entidade Stock usando FluentValidation.
    /// Define regras de validação para items de inventário, incluindo validações condicionais para estado SoldOut.
    /// </summary>
    public class StockValidation : AbstractValidator<Stock>
    {
        /// <summary>
        /// Inicializa as regras de validação para Stock.
        /// Valida referência, produto associado, estado e regras condicionais para vendas definitivas.
        /// </summary>
        public StockValidation()
        {
            // --- Primary Key ---
            RuleFor(s => s.Ref)
                .NotEmpty().WithMessage("The Stock Reference ({PropertyName}) is mandatory!");

            // --- Foreign Key ---
            RuleFor(s => s.ProductId)
                .NotEmpty().WithMessage("The Product ID ({PropertyName}) is mandatory! A stock item must be linked to a product.");

            // --- Enum Status Check ---
            RuleFor(s => s.StockStatus)
                .IsInEnum().WithMessage("The Stock Status ({PropertyName}) has an invalid value.");

            // --- Conditional Rules ---

            // Need to ensure 'SoldOut' status only applies when the stock is already in an Order.
            When(s => s.StockStatus == StockStatus.SoldOut, () =>
            {
                RuleFor(s => s.Orders)
                    // Handle null Orders collection
                    .Must(orders => orders != null && orders.Any())
                    .WithMessage("A Stock item cannot be set to 'SoldOut' unless it has been linked to an Order.")

                    // Handle null Orders collection
                    .Must(orders => orders != null && orders.Any(o => o.Type == OrderType.Sale))
                    .WithMessage("A Stock item can only be set to 'SoldOut' if it is linked to a permanent Sale order, not a Rent order.");
            });
        }
    }
}