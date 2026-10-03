using LojaMaquinas.Business.Models.Entities;



namespace LojaMaquinas.Business.Interfaces.Entities
    {
    /// <summary>
    /// Interface de serviço para gestão de histórico.
    /// </summary>
    internal interface IHistoryService : IDisposable
        {
        /// <summary>
        /// Obtém um registo por identificador.
        /// </summary>
        /// <param name="id">Identificador do registo</param>
        /// <returns>Cliente associado ao registo</returns>
        Task<Client> GetByIdAsync(int id);

        /// <summary>
        /// Obtém um registo por email.
        /// </summary>
        /// <param name="email">Endereço de email</param>
        /// <returns>Cliente associado ao email</returns>
        Task<Client> GetByEmailAsync(string email);

        /// <summary>
        /// Obtém todos os registos.
        /// </summary>
        /// <returns>Coleção de clientes</returns>
        Task<IEnumerable<Client>> GetAllAsync();

        /// <summary>
        /// Cria um novo registo.
        /// </summary>
        /// <param name="user">Cliente a ser criado</param>
        /// <returns>Cliente criado</returns>
        Task<Client> CreateAsync(Client user);

        /// <summary>
        /// Atualiza um registo existente.
        /// </summary>
        /// <param name="user">Cliente com os dados atualizados</param>
        /// <returns>Cliente atualizado</returns>
        Task<Client> UpdateAsync(Client user);

        /// <summary>
        /// Remove um registo.
        /// </summary>
        /// <param name="id">Identificador do registo a remover</param>
        /// <returns>True se removido com sucesso</returns>
        Task<bool> DeleteAsync(int id);

        /// <summary>
        /// Desativa um registo.
        /// </summary>
        /// <param name="id">Identificador do registo a desativar</param>
        /// <returns>True se desativado com sucesso</returns>
        Task<bool> DeactivateAsync(int id);

        /// <summary>
        /// Verifica se um email já existe no histórico.
        /// </summary>
        /// <param name="email">Endereço de email a verificar</param>
        /// <returns>True se o email já existe</returns>
        Task<bool> EmailExistsAsync(string email);
        }
    }
