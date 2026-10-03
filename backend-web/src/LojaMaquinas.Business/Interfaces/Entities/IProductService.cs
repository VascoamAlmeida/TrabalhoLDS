using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.Business.Interfaces.Parameters
{
    /// <summary>
    /// Interface de serviço para gestão de produtos.
    /// Contém a lógica de negócio e validações relacionadas com produtos.
    /// </summary>
    public interface IProductService : IDisposable
    {
        /// <summary>
        /// Adiciona um novo produto ao sistema.
        /// Valida os dados e verifica duplicações antes de persistir.
        /// </summary>
        /// <param name="product">Produto a ser adicionado</param>
        Task Add(Product product);

        /// <summary>
        /// Atualiza um produto existente.
        /// Valida os dados e verifica se o produto existe antes de atualizar.
        /// </summary>
        /// <param name="product">Produto com os dados atualizados</param>
        Task Update(Product product);

        /// <summary>
        /// Remove um produto do sistema (soft delete).
        /// Desativa o produto e marca os seus stocks como não disponíveis.
        /// </summary>
        /// <param name="id">Identificador único do produto a remover</param>
        Task Remove(Guid id);

        /// <summary>
        /// Obtém produtos com stock abaixo do limiar definido (5 unidades).
        /// Útil para alertas de reposição de stock.
        /// </summary>
        /// <returns>Coleção de produtos com stock baixo</returns>
        Task<IEnumerable<Product>> GetLowStockProducts();
    }
}
