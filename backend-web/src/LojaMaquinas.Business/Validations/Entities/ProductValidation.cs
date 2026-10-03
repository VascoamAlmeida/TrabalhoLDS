using FluentValidation;
using LojaMaquinas.Business.Models.Entities;
using System;

namespace LojaMaquinas.Business.Validations.Entities
{
    /// <summary>
    /// Validador para a entidade Product usando FluentValidation.
    /// Define regras de validação para informações de produtos e equipamentos.
    /// </summary>
    public class ProductValidation : AbstractValidator<Product>
    {
        /// <summary>
        /// Inicializa as regras de validação para Product.
        /// Valida nome, marca, modelo, descrição, imagem, tipo, categoria, ano de fabrico, preço e quantidade.
        /// </summary>
        public ProductValidation()
        {
            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .Length(3, 50).WithMessage("The {PropertyName} must be between {MinLength} and {MaxLength} characters");

            RuleFor(p => p.Brand)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .Length(2, 30).WithMessage("The {PropertyName} must be between {MinLength} and {MaxLength} characters");

            RuleFor(p => p.Model)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .Length(2, 50).WithMessage("The {PropertyName} must be between {MinLength} and {MaxLength} characters");

            RuleFor(p => p.Description)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .Length(10, 500).WithMessage("The {PropertyName} must be between {MinLength} and {MaxLength} characters");

            RuleFor(p => p.Image)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .Length(5, 200).WithMessage("The {PropertyName} must be between {MinLength} and {MaxLength} characters");

            RuleFor(p => p.Type)
                .IsInEnum().WithMessage("The {PropertyName} has an invalid value.");

            RuleFor(p => p.CategoryId)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!");

            RuleFor(p => p.YearManufacture)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .GreaterThan(1900).WithMessage("The {PropertyName} must be greater than 1900")
                .LessThanOrEqualTo(DateTime.Now.Year).WithMessage("The {PropertyName} cannot be in the future");

            RuleFor(p => p.Price)
                .NotEmpty().WithMessage("The {PropertyName} is mandatory!")
                .GreaterThan(0).WithMessage("The {PropertyName} must be greater than zero");

            RuleFor(p => p.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("The {PropertyName} must be a non-negative number");
        }
    }
}