using System.Security.Claims;
using LojaMaquinas.Business.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LojaMaquinas.WebApp.Controllers.API
{
	/// <summary>
	/// Controller base abstrato para todos os controllers de API REST.
	/// Fornece funcionalidades comuns: validação de operações, gestão de notificações e
	/// formatação padronizada de respostas JSON (success/data ou success/errors).
	/// </summary>
	[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
	[ApiController]
    [Produces("application/json")]
    public abstract class BaseApiController : Controller
    {
        private readonly INotifierService _notifier;

        /// <summary>
        /// Inicializa o controller API base com serviço de notificações.
        /// </summary>
        /// <param name="notifier">Serviço para gestão de notificações/erros</param>
        protected BaseApiController(INotifierService notifier)
        {
            _notifier = notifier;
        }

        /// <summary>
        /// Indica se o utilizador autenticado tem o papel de administrador.
        /// </summary>
        protected bool IsAdmin => User.IsInRole("ADMIN");

        /// <summary>
        /// Obtém o identificador do utilizador autenticado a partir do JWT.
        /// </summary>
        protected Guid? AuthenticatedUserId
        {
            get
            {
                var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return Guid.TryParse(value, out var userId) ? userId : null;
            }
        }

        /// <summary>
        /// Verifica se a operação é válida (sem notificações de erro).
        /// </summary>
        /// <returns>True se operação válida, False se existem erros</returns>
        protected bool ValidOperation()
        {
            return !_notifier.HasNotification();
        }

        /// <summary>
        /// Verifica se não existem notificações de erro de persistência.
        /// </summary>
        /// <returns>True se não há erros, False se existem notificações</returns>
        protected bool PersistenceValidation()
        {
            return !_notifier.HasNotification();
        }

        /// <summary>
        /// Obtém todas as mensagens de erro/notificações pendentes.
        /// </summary>
        /// <returns>Lista de mensagens de erro, ou null se não houver erros</returns>
        protected List<string> PersistenceResponse()
        {
            if (PersistenceValidation()) return null;

            var warnings = _notifier.GetNotification().Select(n => n.Message).ToList();

            return warnings;
        }

        /// <summary>
        /// Retorna uma resposta HTTP padronizada em formato JSON.
        /// Em caso de sucesso: { success: true, data: {...} }
        /// Em caso de erro: { success: false, errors: ["..."] }
        /// </summary>
        /// <param name="result">Objeto de dados a retornar em caso de sucesso</param>
        /// <returns>ActionResult com resposta HTTP 200 (OK) ou 400 (BadRequest)</returns>
        protected IActionResult CustomResponse(object result = null)
        {
            if (ValidOperation())
            {
                return Ok(new
                {
                    success = true,
                    data = result
                });
            }

            return BadRequest(new
            {
                success = false,
                errors = _notifier.GetNotification().Select(n => n.Message)
            });
        }
    }
}