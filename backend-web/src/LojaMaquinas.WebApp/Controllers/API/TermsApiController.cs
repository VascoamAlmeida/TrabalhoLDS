using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Interfaces;
using Microsoft.AspNetCore.Mvc;
using LojaMaquinas.WebApp.ViewModels.MVC;

namespace LojaMaquinas.WebApp.Controllers.API
{
    /// <summary>
    /// API Controller para obtenção de termos e condições.
    /// Fornece endpoint para consultar o template atual de termos e condições.
    /// </summary>
    [Route("api/v1/terms")]
    [ApiController]
    public class TermsApiController : BaseApiController
    {
        private readonly ITermsService _termsService;

        /// <summary>
        /// Inicializa o controller de termos e condições.
        /// </summary>
        /// <param name="termsService">Serviço para gestão de termos e condições</param>
        /// <param name="notifier">Serviço de notificações de erro</param>
        public TermsApiController(ITermsService termsService, INotifierService notifier) : base(notifier)
        {
            _termsService = termsService;
        }


        /// <summary>
        /// Obtém o template atual de termos e condições.
        /// GET: api/v1/terms
        /// </summary>
        /// <returns>Conteúdo dos termos e condições</returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var content = await _termsService.GetTermsAndConditions();
            return Ok(new TermsViewModel { Content = content });
        }
    }
}
