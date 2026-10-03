using LojaMaquinas.Business.Models;

namespace LojaMaquinas.Business.Interfaces
{
    /// <summary>
    /// Interface para serviço de notificações do sistema.
    /// Gerencia notificações de erros e validações durante a execução de operações de negócio.
    /// </summary>
    public interface INotifierService
    {
        /// <summary>
        /// Verifica se existem notificações registadas.
        /// </summary>
        /// <returns>True se existirem notificações, False caso contrário</returns>
        bool HasNotification();

        /// <summary>
        /// Obtém todas as notificações registadas.
        /// </summary>
        /// <returns>Lista de notificações</returns>
        List<Notification> GetNotification();

        /// <summary>
        /// Adiciona uma nova notificação ao sistema.
        /// </summary>
        /// <param name="notification">Notificação a ser adicionada</param>
        void Handle(Notification notification);
    }
}
