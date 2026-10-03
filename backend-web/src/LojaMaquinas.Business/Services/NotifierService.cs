using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Models;

namespace LojaMaquinas.Business.Services
{
    /// <summary>
    /// Implementação do serviço de notificações do sistema.
    /// Gere uma lista de notificações de erro e validação durante as operações.
    /// </summary>
    public class NotifierService : INotifierService
    {
        private List<Notification> _notifications;

        /// <summary>
        /// Inicializa uma nova instância do serviço de notificações.
        /// Cria uma lista vazia para armazenar as notificações.
        /// </summary>
        public NotifierService()
        {
            _notifications = new List<Notification>();
        }

        /// <summary>
        /// Obtém todas as notificações registadas.
        /// </summary>
        /// <returns>Lista de notificações acumuladas</returns>
        public List<Notification> GetNotification()
        {
            return _notifications;
        }

        /// <summary>
        /// Adiciona uma nova notificação à lista.
        /// </summary>
        /// <param name="notification">Notificação a ser adicionada</param>
        public void Handle(Notification notification)
        {
            _notifications.Add(notification);
        }

        /// <summary>
        /// Verifica se existem notificações registadas.
        /// </summary>
        /// <returns>True se existirem notificações, False caso contrário</returns>
        public bool HasNotification()
        {
            return _notifications.Any();
        }
    }
}
