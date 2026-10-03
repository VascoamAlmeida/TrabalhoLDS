namespace LojaMaquinas.Business.Models
{
    /// <summary>
    /// Representa uma notificação de erro ou validação no sistema.
    /// Utilizada pelo NotifierService para comunicar problemas durante operações de negócio.
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// Inicializa uma nova notificação com a mensagem especificada.
        /// </summary>
        /// <param name="message">Mensagem descritiva da notificação</param>
        public Notification(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Mensagem descritiva da notificação.
        /// Contém detalhes sobre o erro ou validação.
        /// </summary>
        public string Message { get; set; }
    }
}
