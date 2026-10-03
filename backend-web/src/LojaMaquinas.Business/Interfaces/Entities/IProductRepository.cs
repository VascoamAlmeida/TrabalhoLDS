using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;

namespace LojaMaquinas.Business.Interfaces.Parameters
{
    /// <summary>
    /// Interface de repositório para gestão de produtos na base de dados.
    /// Fornece operações específicas para consulta e manipulação de produtos.
    /// </summary>
    public interface IProductRepository : IRepository<Product>
    {
        /// <summary>
        /// Obtém um produto com todos os seus stocks associados.
        /// </summary>
        /// <param name="id">Identificador único do produto</param>
        /// <returns>Produto com a coleção de stocks carregada</returns>
        Task<Product> GetStocksByProductId(Guid id);

        /// <summary>
        /// Obtém todos os produtos de uma categoria específica.
        /// </summary>
        /// <param name="categoryId">Identificador da categoria</param>
        /// <returns>Coleção de produtos pertencentes à categoria</returns>
        Task<IEnumerable<Product>> GetProductsByCategory(Guid categoryId);

        /// <summary>
        /// Obtém um produto com todos os detalhes e relações carregadas (eager loading).
        /// </summary>
        /// <param name="id">Identificador único do produto</param>
        /// <returns>Produto com todas as relações carregadas</returns>
        Task<Product> GetProductWithAllDetails(Guid id);

        /// <summary>
        /// Obtém todos os produtos de um tipo específico.
        /// </summary>
        /// <param name="type">Tipo de produto (enum ProductType)</param>
        /// <returns>Coleção de produtos do tipo especificado</returns>
        Task<IEnumerable<Product>> GetProductsByType(ProductType type);

        /// <summary>
        /// Obtém todos os produtos que estão disponíveis (IsActive = true).
        /// </summary>
        /// <returns>Coleção de produtos disponíveis</returns>
        Task<IEnumerable<Product>> GetAvailableProducts();

        /// <summary>
        /// Pesquisa produtos por termo de busca (nome, marca, modelo, descrição).
        /// </summary>
        /// <param name="searchTerm">Termo de pesquisa</param>
        /// <returns>Coleção de produtos que correspondem à pesquisa</returns>
        Task<IEnumerable<Product>> SearchProducts(string searchTerm);

        /// <summary>
        /// Obtém produtos com quantidade em stock abaixo do limiar especificado.
        /// </summary>
        /// <param name="threshold">Limiar de stock baixo</param>
        /// <returns>Coleção de produtos com stock baixo</returns>
        Task<IEnumerable<Product>> GetProductsWithLowStock(int threshold);

        /// <summary>
        /// Devolve todos os produtos com a categoria associada.
        /// </summary>
        /// <returns>Produtos com Categoria carregada</returns>
        Task<IEnumerable<Product>> GetAllProductsWithCategory();

        /// <summary>
        /// Devolve todos os produtos com a categoria associada e stocks associados.
        /// </summary>
        /// <returns>Produtos com Categoria e stocks carregada</returns>
        Task<IEnumerable<Product>> GetAllProductsWithCategoryAndStocks();
    }
}
