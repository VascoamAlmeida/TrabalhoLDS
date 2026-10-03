using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Data.Contexts;
using Microsoft.EntityFrameworkCore;

namespace LojaMaquinas.Data.Repositories.Entities
{
    /// <summary>
    /// Repositório para acesso aos dados de utilizadores na base de dados.
    /// Fornece métodos para autenticação e gestão de utilizadores.
    /// </summary>
    public class UserRepository : Repository<User>, IUserRepository
    {
        /// <summary>
        /// Inicializa uma nova instância do repositório de utilizadores.
        /// </summary>
        /// <param name="context">Contexto da base de dados</param>
        public UserRepository(MyDbContext context) : base(context) { }

        /// <summary>
        /// Obtém um utilizador com o seu cliente associado (eager loading).
        /// </summary>
        /// <param name="id">Identificador único do utilizador</param>
        /// <returns>Utilizador com Client carregado ou null se não existir</returns>
        public async Task<User> GetUserWithClient(Guid id)
        {
            return await Db.E_Users.AsNoTracking()
                                    .Include(u => u.Client)
                                    .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Procura um utilizador pelo seu endereço de email.
        /// Utilizado para validação de duplicados e autenticação.
        /// </summary>
        /// <param name="email">Endereço de email do utilizador</param>
        /// <returns>Utilizador encontrado ou null se não existir</returns>
        public async Task<User> GetByEmail(string email)
        {
            return await Db.E_Users.AsNoTracking()
                                    .FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <summary>
        /// Obtém todos os utilizadores ativos no sistema.
        /// Filtra utilizadores com IsActive = true.
        /// </summary>
        /// <returns>Coleção de utilizadores ativos</returns>
        public async Task<IEnumerable<User>> GetAllActiveUsers()
        {
            return await Db.E_Users.AsNoTracking()
                                    .Where(u => u.IsActive)
                                    .ToListAsync();
        }
    }
}
