using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Business.Models.Parameters;

namespace LojaMaquinas.Business.Models.Entities
{
    /// <summary>
    /// Representa um produto disponível no sistema de loja de máquinas.
    /// Contém informações sobre equipamentos para venda ou aluguer.
    /// </summary>
    public class Product : Entity
    {
        /// <summary>
        /// Nome do produto.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Tipo de produto (Sale ou Rent).
        /// Define se o produto está disponível para venda, aluguer ou ambos.
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
        /// Preço do produto (venda ou aluguer diário).
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Quantidade total de unidades disponíveis em stock.
        /// Calculado automaticamente com base nos registos de Stock.
        /// </summary>
        public int Quantity { get; set; }

        /// <summary>
        /// URL ou caminho da imagem do produto.
        /// </summary>
        public string Image { get; set; }

        /// <summary>
        /// Identificador da categoria a que o produto pertence.
        /// Chave estrangeira para a tabela Category.
        /// </summary>
        public Guid CategoryId { get; set; }

        /// <summary>
        /// Indica se o produto está ativo no sistema.
        /// Produtos inativos não são apresentados aos clientes.
        /// </summary>
        public bool IsActive { get; set; }

        /* EF Relations */

        /// <summary>
        /// Categoria a que este produto pertence.
        /// Navigation property do Entity Framework.
        /// </summary>
        public Category Category { get; set; }

        /// <summary>
        /// Coleção de items de stock associados a este produto.
        /// Navigation property do Entity Framework.
        /// </summary>
        public IEnumerable<Stock> Stocks { get; set; }

    }
}