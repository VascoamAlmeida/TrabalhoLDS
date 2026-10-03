using Azure.Core;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace LojaMaquinas.WebApp.Controllers
{
    /// <summary>
    /// Controller base abstrato para todos os controllers MVC da aplicação.
    /// Fornece funcionalidades comuns: validação, notificações de erro e gestão de perfis de utilizador.
    /// Excluído da documentação da API Swagger.
    /// </summary>
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    public abstract class BaseController : Controller
    {
        private readonly INotifierService _notifier;

        /// <summary>
        /// Inicializa o controller base com serviço de notificações.
        /// </summary>
        /// <param name="notifier">Serviço para gestão de notificações/erros</param>
        protected BaseController(INotifierService notifier)
        {
            _notifier = notifier;
        }

        /// <summary>
        /// Verifica se não existem notificações de erro pendentes.
        /// </summary>
        /// <returns>True se não há erros, False se existem notificações</returns>
        protected bool PersistenceValidation()
        {
            return !_notifier.HasNotification();
        }

        /// <summary>
        /// Obtém todas as mensagens de erro/notificações pendentes.
        /// Utilizado para exibir erros de persistência ao utilizador.
        /// </summary>
        /// <returns>Lista de mensagens de erro, ou null se não houver erros</returns>
        protected List<string> PersistenceResponse()
        {
            if (PersistenceValidation()) return null;

            var warnings = _notifier.GetNotification().Select(n => n.Message).ToList();

            //foreach (var warning in warnings) AuditLog(LogType.Warning, warning);

            return warnings;
        }

        /// <summary>
        /// Adiciona uma notificação de erro ao serviço de notificações.
        /// </summary>
        /// <param name="message">Mensagem de erro a notificar</param>
        protected void NotifyErrorMessage(string message)
        {
            _notifier.Handle(new Notification(message));
        }

        /// <summary>
        /// Obtém todos os erros de validação do ModelState.
        /// Utilizado para exibir erros de validação de formulários.
        /// </summary>
        /// <returns>Lista de mensagens de erro do ModelState, ou null se válido</returns>
        protected List<string> ModelStateResponse()
        {
            if (!ModelState.IsValid)
            {
                var warnings = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                //foreach (var warning in warnings) AuditLog(LogType.Warning, warning);

                return warnings;
            }

            return null;
        }

        //protected void AuditLog(LogType logType, string description)
        //{
        //    //_audit.RegisterLog(HttpContext, Convert.ToInt32(logType), description);
        //}

        /// <summary>
        /// Executado antes de cada Action em todos os controllers derivados.
        /// Extrai informações de autenticação dos cookies (UserType, UserEmail) e
        /// disponibiliza-as nas Views através de ViewBag/ViewData para controlo de acesso.
        /// </summary>
        /// <param name="context">Contexto de execução da Action</param>
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var profileCookie = Request.Cookies["UserType"];
            var emailCookie = Request.Cookies["UserEmail"];

            var profile = profileCookie?.Trim();
            var email = emailCookie?.Trim();

            // 🔸 Só é admin se o perfil for exatamente "Admin" (case-insensitive)
            var isAdmin = string.Equals(profile, "Admin", StringComparison.OrdinalIgnoreCase);

            // 🔸 Só é user se o perfil for exatamente "User" (case-insensitive)
            var isClient = string.Equals(profile, "Client", StringComparison.OrdinalIgnoreCase);

            // 🔹 Define as variáveis disponíveis nas Views
            ViewBag.Profile = profile;
            ViewBag.UserEmail = email;
            ViewBag.IsAdmin = isAdmin;
            ViewBag.IsClient = isClient;
            ViewData["IsAdmin"] = isAdmin;
            ViewData["IsClient"] = isClient;

            base.OnActionExecuting(context);
        }

    }
}