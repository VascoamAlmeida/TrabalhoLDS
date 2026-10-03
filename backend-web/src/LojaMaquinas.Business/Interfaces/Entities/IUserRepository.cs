using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.Business.Interfaces.Entities
    {
    /// <summary>
    /// Interface de repositório para gestão de utilizadores na base de dados.
    /// Fornece operações específicas para consulta de utilizadores e autenticação.
    /// </summary>
    public interface IUserRepository : IRepository<User>
        {
        /// <summary>
        /// Obtém um utilizador com os dados do cliente associado.
        /// </summary>
        /// <param name="id">Identificador único do utilizador</param>
        /// <returns>Utilizador com a relação Client carregada</returns>
        Task<User> GetUserWithClient(Guid id);

        /// <summary>
        /// Obtém um utilizador pelo endereço de email.
        /// Útil para autenticação e verificação de duplicações.
        /// </summary>
        /// <param name="email">Endereço de email do utilizador</param>
        /// <returns>Utilizador com o email especificado ou null se não existir</returns>
        Task<User> GetByEmail(string email);

        /// <summary>
        /// Obtém todos os utilizadores ativos no sistema.
        /// </summary>
        /// <returns>Coleção de utilizadores ativos</returns>
        Task<IEnumerable<User>> GetAllActiveUsers();
        }
    }
