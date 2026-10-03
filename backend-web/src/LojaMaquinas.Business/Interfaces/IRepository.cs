using LojaMaquinas.Business.Models;
using System.Linq.Expressions;

namespace LojaMaquinas.Business.Interfaces
{
    /// <summary>
    /// Interface genérica base para todos os repositórios do sistema.
    /// Define operações CRUD (Create, Read, Update, Delete) padrão para entidades.
    /// </summary>
    /// <typeparam name="TEntity">Tipo da entidade que deve herdar de Entity</typeparam>
    public interface IRepository<TEntity> : IDisposable where TEntity : Entity
    {
        /// <summary>
        /// Obtém todas as entidades da base de dados.
        /// </summary>
        /// <returns>Lista com todas as entidades do tipo TEntity</returns>
        Task<List<TEntity>> GetAll();

        /// <summary>
        /// Obtém uma entidade específica pelo seu identificador único.
        /// </summary>
        /// <param name="id">Identificador único (GUID) da entidade</param>
        /// <returns>A entidade encontrada ou null se não existir</returns>
        Task<TEntity> GetById(Guid id);

        /// <summary>
        /// Procura entidades que satisfazem um predicado/condição específica.
        /// </summary>
        /// <param name="predicate">Expressão lambda que define a condição de pesquisa</param>
        /// <returns>Coleção de entidades que satisfazem o predicado</returns>
        Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> predicate);

        /// <summary>
        /// Adiciona uma nova entidade à base de dados.
        /// </summary>
        /// <param name="entity">Entidade a ser adicionada</param>
        Task Add(TEntity entity);

        /// <summary>
        /// Atualiza uma entidade existente na base de dados.
        /// </summary>
        /// <param name="entity">Entidade com os dados atualizados</param>
        Task Update(TEntity entity);

        /// <summary>
        /// Remove uma entidade da base de dados pelo seu identificador.
        /// </summary>
        /// <param name="id">Identificador único (GUID) da entidade a remover</param>
        Task Remove(Guid id);

        /// <summary>
        /// Persiste todas as alterações pendentes na base de dados.
        /// </summary>
        /// <returns>Número de registos afetados pela operação</returns>
        Task<int> SaveChanges();
    }
}
