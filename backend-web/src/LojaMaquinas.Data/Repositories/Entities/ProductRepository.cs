using LojaMaquinas.Business.Interfaces.Parameters;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LojaMaquinas.Data.Repositories.Parameters
{
    /// <summary>
    /// Repositório para acesso aos dados de produtos na base de dados.
    /// Fornece métodos avançados para pesquisa e filtragem de produtos.
    /// </summary>
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        /// <summary>
        /// Inicializa uma nova instância do repositório de produtos.
        /// </summary>
        /// <param name="context">Contexto da base de dados</param>
        public ProductRepository(MyDbContext context) : base(context) { }

        /// <summary>
        /// Obtém um produto com todos os seus items de stock (eager loading).
        /// </summary>
        /// <param name="id">Identificador único do produto</param>
        /// <returns>Produto com Stocks carregados ou null se não existir</returns>
        public async Task<Product> GetStocksByProductId(Guid id)
        {
            return await Db.E_Products.AsNoTracking()
                                        .Include(p => p.Stocks)
                                        .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Obtém todos os produtos de uma categoria específica.
        /// </summary>
        /// <param name="categoryId">Identificador único da categoria</param>
        /// <returns>Coleção de produtos da categoria especificada</returns>
        public async Task<IEnumerable<Product>> GetProductsByCategory(Guid categoryId)
        {
            return await Db.E_Products.AsNoTracking()
                                      .Where(p => p.CategoryId == categoryId)
                                      .ToListAsync();
        }

        /// <summary>
        /// Obtém um produto com todos os detalhes completos (categoria e stocks).
        /// </summary>
        /// <param name="id">Identificador único do produto</param>
        /// <returns>Produto com Category e Stocks carregados ou null se não existir</returns>
        public async Task<Product> GetProductWithAllDetails(Guid id)
        {
            return await Db.E_Products.AsNoTracking()
                                      .Include(p => p.Category)
                                      .Include(p => p.Stocks)
                                      .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Devolve todos os produtos com a categoria associada.
        /// </summary>
        /// <returns>Produtos com Categoria carregada</returns>
        public async Task<IEnumerable<Product>> GetAllProductsWithCategory()
        {
            return await Db.E_Products.AsNoTracking()
                                     .Include(s => s.Category)
                                     .ToListAsync();
        }

        /// <summary>
        /// Devolve todos os produtos com a categoria associada e stocks associados.
        /// </summary>
        /// <returns>Produtos com Categoria e stocks carregada</returns>
        public async Task<IEnumerable<Product>> GetAllProductsWithCategoryAndStocks()
        {
            return await Db.E_Products.AsNoTracking()
                                     .Include(s => s.Category)
                                     .Include(s => s.Stocks)
                                     .ToListAsync();
        }

        /// <summary>
        /// Obtém produtos filtrados por tipo (Machine ou Part).
        /// </summary>
        /// <param name="type">Tipo de produto a filtrar</param>
        /// <returns>Coleção de produtos do tipo especificado</returns>
        public async Task<IEnumerable<Product>> GetProductsByType(ProductType type)
        {
            return await Db.E_Products.AsNoTracking()
                                      .Where(p => p.Type == type)
                                      .ToListAsync();
        }

        /// <summary>
        /// Obtém produtos que têm pelo menos um item de stock disponível.
        /// Útil para mostrar apenas produtos em stock na loja.
        /// </summary>
        /// <returns>Coleção de produtos com stock disponível</returns>
        public async Task<IEnumerable<Product>> GetAvailableProducts()
        {
            return await Db.E_Products.AsNoTracking()
                                      .Where(p => p.Stocks.Any(s => s.StockStatus == StockStatus.Available))
                                      .ToListAsync();
        }

        /// <summary>
        /// Pesquisa produtos por nome (case-insensitive).
        /// </summary>
        /// <param name="searchTerm">Termo de pesquisa</param>
        /// <returns>Coleção de produtos cujo nome contém o termo pesquisado</returns>
        public async Task<IEnumerable<Product>> SearchProducts(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return Enumerable.Empty<Product>();
            }

            string normalizedTerm = searchTerm.Trim().ToLower();

            return await Db.E_Products.AsNoTracking()
                                      .Where(p => p.Name.ToLower().Contains(normalizedTerm))
                                      .ToListAsync();
        }

        /// <summary>
        /// Obtém produtos com stock abaixo do limiar especificado.
        /// Utilizado para alertas de reposição de stock.
        /// </summary>
        /// <param name="threshold">Limiar de stock (número mínimo de unidades disponíveis)</param>
        /// <returns>Coleção de produtos com stock baixo</returns>
        public async Task<IEnumerable<Product>> GetProductsWithLowStock(int threshold)
        {
            return await Db.E_Products.AsNoTracking()
                                     .Include(p => p.Stocks)
                                     .Where(p => p.Stocks.Count(s => s.StockStatus == StockStatus.Available) < threshold)
                                     .ToListAsync();
        }
    }
}
