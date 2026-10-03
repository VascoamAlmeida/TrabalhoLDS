using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.Business.Interfaces.Entities
{
    /// <summary>
    /// Interface de serviço para gestão de utilizadores.
    /// Contém a lógica de negócio para criação de contas e autenticação.
    /// </summary>
    public interface IUserService : IDisposable
    {
        /// <summary>
        /// Adiciona um novo utilizador ao sistema.
        /// Valida os dados, encripta a password e verifica duplicações de email.
        /// </summary>
        /// <param name="user">Utilizador a ser adicionado</param>
        Task Add(User user);

        /// <summary>
        /// Atualiza um utilizador existente.
        /// Valida os dados e verifica se o utilizador existe antes de atualizar.
        /// </summary>
        /// <param name="user">Utilizador com os dados atualizados</param>
        Task Update(User user);

        /// <summary>
        /// Remove um utilizador do sistema.
        /// </summary>
        /// <param name="id">Identificador único do utilizador a remover</param>
        Task Remove(Guid id);
    }
}
