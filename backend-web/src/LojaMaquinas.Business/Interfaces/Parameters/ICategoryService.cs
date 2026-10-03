using LojaMaquinas.Business.Models.Parameters;

namespace LojaMaquinas.Business.Interfaces.Parameters
{
    /// <summary>
    /// Interface de serviço para gestão de categorias de produtos.
    /// Contém a lógica de negócio e validações para categorias.
    /// </summary>
    public interface ICategoryService : IDisposable
    {
        /// <summary>
        /// Adiciona uma nova categoria ao sistema.
        /// Valida os dados e verifica duplicações antes de persistir.
        /// </summary>
        /// <param name="category">Categoria a ser adicionada</param>
        Task Add(Category category);

        /// <summary>
        /// Atualiza uma categoria existente.
        /// Valida os dados e verifica se a categoria existe antes de atualizar.
        /// </summary>
        /// <param name="category">Categoria com os dados atualizados</param>
        Task Update(Category category);

        /// <summary>
        /// Remove uma categoria do sistema.
        /// Remove também os produtos associados à categoria.
        /// </summary>
        /// <param name="id">Identificador único da categoria a remover</param>
        Task Remove(Guid id);
    }
}
