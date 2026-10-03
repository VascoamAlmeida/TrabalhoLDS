using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Models;
using LojaMaquinas.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace LojaMaquinas.Data.Repositories
{
    /// <summary>
    /// Classe base abstrata para todos os repositórios do sistema.
    /// Implementa operações CRUD genéricas usando Entity Framework Core.
    /// </summary>
    /// <typeparam name="TEntity">Tipo da entidade gerida pelo repositório (deve herdar de Entity)</typeparam>
    public abstract class Repository<TEntity> : IRepository<TEntity> where TEntity : Entity, new()
    {
        /// <summary>
        /// Contexto da base de dados Entity Framework.
        /// </summary>
        protected readonly MyDbContext Db;

        /// <summary>
        /// DbSet da entidade TEntity para acesso direto à tabela.
        /// </summary>
        protected readonly DbSet<TEntity> DbSet;

        /// <summary>
        /// Inicializa uma nova instância do repositório.
        /// </summary>
        /// <param name="db">Contexto da base de dados</param>
        protected Repository(MyDbContext db)
        {
            Db = db;
            DbSet = db.Set<TEntity>();
        }

        /// <summary>
        /// Procura entidades que correspondam ao predicado especificado.
        /// Utiliza AsNoTracking para melhor performance em queries de leitura.
        /// </summary>
        /// <param name="predicate">Expressão lambda para filtrar as entidades</param>
        /// <returns>Coleção de entidades que correspondem ao predicado</returns>
        public async Task<IEnumerable<TEntity>> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return await DbSet.AsNoTracking().Where(predicate).ToListAsync();
        }

        /// <summary>
        /// Obtém uma entidade pelo seu identificador único.
        /// </summary>
        /// <param name="id">Identificador único da entidade (GUID)</param>
        /// <returns>Entidade encontrada ou null se não existir</returns>
        public virtual async Task<TEntity> GetById(Guid id)
        {
            return await DbSet.FindAsync(id);
        }

        /// <summary>
        /// Obtém todas as entidades da tabela.
        /// </summary>
        /// <returns>Lista com todas as entidades</returns>
        public virtual async Task<List<TEntity>> GetAll()
        {
            return await DbSet.ToListAsync();
        }

        /// <summary>
        /// Adiciona uma nova entidade à base de dados.
        /// Guarda as alterações automaticamente após adicionar.
        /// </summary>
        /// <param name="entity">Entidade a ser adicionada</param>
        public virtual async Task Add(TEntity entity)
        {
            DbSet.Add(entity);
            await SaveChanges();
        }

        /// <summary>
        /// Atualiza uma entidade existente na base de dados.
        /// Guarda as alterações automaticamente após atualizar.
        /// </summary>
        /// <param name="entity">Entidade com os dados atualizados</param>
        public virtual async Task Update(TEntity entity)
        {
            DbSet.Update(entity);
            await SaveChanges();
        }

        /// <summary>
        /// Remove uma entidade da base de dados pelo seu identificador.
        /// Guarda as alterações automaticamente após remover.
        /// </summary>
        /// <param name="id">Identificador único da entidade a remover</param>
        public virtual async Task Remove(Guid id)
        {
            DbSet.Remove(new TEntity { Id = id });
            await SaveChanges();
        }

        /// <summary>
        /// Persiste todas as alterações pendentes na base de dados.
        /// </summary>
        /// <returns>Número de registos afetados</returns>
        public async Task<int> SaveChanges()
        {
            return await Db.SaveChangesAsync();
        }

        /// <summary>
        /// Liberta os recursos utilizados pelo contexto da base de dados.
        /// </summary>
        public void Dispose()
        {
            Db?.Dispose();
        }
    }
}
