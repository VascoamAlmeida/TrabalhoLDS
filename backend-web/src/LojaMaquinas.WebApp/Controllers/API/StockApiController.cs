using AutoMapper;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Parameters;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Helper.StaticVariables;
using LojaMaquinas.WebApp.ViewModels.MVC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LojaMaquinas.WebApp.Controllers.API
{
    /// <summary>
    /// API Controller para gestão de stock/inventário.
    /// Fornece endpoints CRUD completos + processamento de devoluções de alugueres e cancelamentos.
    /// </summary>
    [Route("api/v1/stock")]
    [ApiController]
    public class StockApiController : BaseApiController
    {
        private readonly IStockRepository _stockRepository;
        private readonly IStockService _stockService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Inicializa o controller de stock com dependências necessárias.
        /// </summary>
        /// <param name="stockRepository">Repositório de acesso a dados de stock</param>
        /// <param name="stockService">Serviço de lógica de negócio de stock</param>
        /// <param name="mapper">AutoMapper para conversão entre entidades e ViewModels</param>
        /// <param name="notifier">Serviço de notificações de erro</param>
        public StockApiController(IStockRepository stockRepository,
                                  IStockService stockService,
                                  IMapper mapper,
                                  INotifierService notifier) : base(notifier)
        {
            _stockRepository = stockRepository;
            _stockService = stockService;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtém todos os items de stock.
        /// GET: api/v1/stock
        /// </summary>
        /// <returns>Lista de todos os items de stock</returns>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var stocks = await _stockRepository.GetAll();

            var viewModels = _mapper.Map<IEnumerable<StockApiViewModel>>(stocks);
            return Ok(viewModels);
        }

        /// <summary>
        /// Obtém detalhes de um item de stock específico por ID.
        /// GET: api/v1/stock/{id}
        /// </summary>
        /// <param name="id">ID único do item de stock</param>
        /// <returns>Item de stock encontrado ou NotFound</returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var stockViewModel = await GetStockDetailsById(id);

            if (stockViewModel == null)
                return NotFound(new { success = false, message = "Stock item not found." });

            var viewModel = _mapper.Map<StockApiViewModel>(stockViewModel);
            return Ok(viewModel);
        }

        #region Creation Methods
        /// <summary>
        /// Cria um novo item de stock.
        /// POST: api/v1/stock
        /// </summary>
        /// <param name="stockViewModel">Dados do item de stock a criar</param>
        /// <returns>Mensagem de sucesso ou erros de validação</returns>
        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] StockApiViewModel stockViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var stock = _mapper.Map<Stock>(stockViewModel);
                await _stockService.Add(stock);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessCreationRegistration("Stock item", stockViewModel.Ref.ToString())
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }
        #endregion

        #region Edit Methods
        /// <summary>
        /// Atualiza um item de stock existente.
        /// PUT: api/v1/stock/{id}
        /// </summary>
        /// <param name="id">ID do item de stock a atualizar</param>
        /// <param name="stockViewModel">Dados atualizados do item de stock</param>
        /// <returns>Mensagem de sucesso ou erros de validação</returns>
        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] StockApiViewModel stockViewModel)
        {
            if (id != stockViewModel.Id)
                return BadRequest(new { success = false, message = "Stock id is invalid." });

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var stock = _mapper.Map<Stock>(stockViewModel);
                await _stockService.Update(stock);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessChangeRegistration("Stock item", stockViewModel.Ref.ToString())
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }
        #endregion

        #region Remove Methods
        /// <summary>
        /// Remove um item de stock.
        /// DELETE: api/v1/stock/{id}
        /// </summary>
        /// <param name="id">ID do item de stock a remover</param>
        /// <returns>Mensagem de sucesso ou erro se item não encontrado</returns>
        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var stock = await _stockRepository.GetById(id);
                if (stock == null)
                    return NotFound(new { success = false, message = "Stock item not found." });

                await _stockService.Remove(id);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessRemovalRegistration("Stock item", stock.Ref.ToString())
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }
        #endregion

        #region Return Methods
        /// <summary>
        /// Processa a devolução de um aluguer (marca stock como disponível novamente).
        /// POST: api/v1/stock/{stockId}/return-rent
        /// </summary>
        /// <param name="stockId">ID do item de stock a devolver</param>
        /// <returns>Mensagem de sucesso ou erro se devolução falhar</returns>
        [Authorize(Roles = "ADMIN")]
        [HttpPost("{stockId:guid}/return-rent")]
        public async Task<IActionResult> ReturnRent(Guid stockId)
        {
            try
            {
                await _stockService.ProcessRentReturn(stockId);
                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, message = "Rent return failed.", errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessChangeRegistration("Rent Return", $"Stock ID {stockId}")
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = $"Internal Server Error during Rent Return: {errorMsg}" });
            }
        }
        /// <summary>
        /// Processa o cancelamento de uma venda (devolução por cancelamento).
        /// Marca stock como disponível após cancelamento da venda.
        /// POST: api/v1/stock/{stockId}/return-cancel
        /// </summary>
        /// <param name="stockId">ID do item de stock cancelado</param>
        /// <returns>Mensagem de sucesso ou erro se cancelamento falhar</returns>
        [Authorize(Roles = "ADMIN")]
        [HttpPost("{stockId:guid}/return-cancel")]
        public async Task<IActionResult> ReturnCancel(Guid stockId)
        {
            try
            {
                await _stockService.ProcessCancelReturn(stockId);
                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, message = "Cancellation return failed.", errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessChangeRegistration("Cancelled Sale Return", $"Stock ID {stockId}")
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = $"Internal Server Error during Sale Cancellation Return: {errorMsg}" });
            }
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Método auxiliar privado para obter detalhes de um item de stock por ID.
        /// </summary>
        /// <param name="id">ID do item de stock</param>
        /// <returns>ViewModel do item de stock ou null se não encontrado</returns>
        private async Task<StockApiViewModel> GetStockDetailsById(Guid id)
        {
            return _mapper.Map<StockApiViewModel>(await _stockRepository.GetById(id));
        }
        #endregion
    }
}
