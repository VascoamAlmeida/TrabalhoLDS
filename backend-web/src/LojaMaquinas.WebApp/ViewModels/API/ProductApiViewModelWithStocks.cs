using LojaMaquinas.Business.Models.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel;

namespace LojaMaquinas.WebApp.ViewModels.MVC
{
    /// <summary>
    /// ViewModel de produto para API REST.
    /// Representa dados completos de produto incluindo preço, quantidade e categoria.
    /// </summary>
    public class ProductApiViewModelWithStocks
    {
        /// <summary>
        /// Identificador único do produto.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Nome do produto.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Tipo de produto (Compra=1, Aluguer=2).
        /// </summary>
        public ProductType Type { get; set; }

        /// <summary>
        /// Marca do produto.
        /// </summary>
        public string Brand { get; set; }

        /// <summary>
        /// Modelo do produto.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Ano de fabrico do produto.
        /// </summary>
        public int YearManufacture { get; set; }

        /// <summary>
        /// Descrição detalhada do produto.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Preço do produto (compra ou aluguer).
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Quantidade total disponível em stock.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// URL ou caminho da imagem do produto.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Indica se o produto está ativo no sistema.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Identificador da categoria à qual o produto pertence.
        /// </summary>
        public Guid CategoryId { get; set; }

        /// <summary>
        /// Coleção de items de stock atuais do produto (navegação).
        /// </summary>
        public IEnumerable<StockApiViewModel>? Stocks { get; set; }
    }
}