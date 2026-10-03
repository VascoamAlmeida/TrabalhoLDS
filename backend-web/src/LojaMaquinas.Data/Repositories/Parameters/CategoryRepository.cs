using LojaMaquinas.Business.Interfaces.Parameters;
using LojaMaquinas.Business.Models.Parameters;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LojaMaquinas.Data.Repositories.Parameters
{
    /// <summary>
    /// Repositório para acesso aos dados de categorias na base de dados.
    /// Fornece métodos para gestão de categorias e relações com produtos.
    /// </summary>
    public class CategoryRepository : Repository<Category>, ICategoryRepository
    {
        /// <summary>
        /// Inicializa uma nova instância do repositório de categorias.
        /// </summary>
        /// <param name="context">Contexto da base de dados</param>
        public CategoryRepository(MyDbContext context) : base(context) { }

        /// <summary>
        /// Obtém uma categoria com todos os seus produtos associados (eager loading).
        /// Útil para visualizar a categoria completa com todos os produtos que pertencem a ela.
        /// </summary>
        /// <param name="id">Identificador único da categoria</param>
        /// <returns>Categoria com Products carregados ou null se não existir</returns>
        public async Task<Category> GetCategoryAndProductByCategoryId(Guid id)
        {
            return await Db.P_Categories.AsNoTracking()
                                        .Include(c => c.Products)
                                        .FirstOrDefaultAsync(c => c.Id == id);
        }

        /// <summary>
        /// Remove um produto específico da base de dados (através da categoria).
        /// Operação de cascata que remove o produto da categoria.
        /// </summary>
        /// <param name="product">Entidade de produto a remover</param>
        public async Task RemoveProductByCategory(Product product)
        {
            Db.E_Products.Remove(product);
            await SaveChanges();
        }
    }
}
