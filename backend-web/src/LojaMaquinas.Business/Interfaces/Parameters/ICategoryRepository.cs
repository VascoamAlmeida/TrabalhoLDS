using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Parameters;

namespace LojaMaquinas.Business.Interfaces.Parameters
{
    /// <summary>
    /// Interface de repositório para gestão de categorias de produtos na base de dados.
    /// Fornece operações específicas para consulta de categorias e produtos associados.
    /// </summary>
    public interface ICategoryRepository : IRepository<Category>
    {
        /// <summary>
        /// Obtém uma categoria com todos os produtos associados.
        /// </summary>
        /// <param name="id">Identificador único da categoria</param>
        /// <returns>Categoria com a coleção de produtos carregada</returns>
        Task<Category> GetCategoryAndProductByCategoryId(Guid id);

        /// <summary>
        /// Remove um produto associado a uma categoria.
        /// </summary>
        /// <param name="product">Entidade Product a ser removida</param>
        Task RemoveProductByCategory(Product product);

    }
}
