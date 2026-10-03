using AutoMapper;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Parameters;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Parameters;
using LojaMaquinas.Data.Repositories.Parameters;
using LojaMaquinas.Helper.StaticVariables;
using LojaMaquinas.WebApp.ViewModels;
using LojaMaquinas.WebApp.ViewModels.MVC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LojaMaquinas.WebApp.Controllers.MVC
{
    /// <summary>
    /// Controller para gestão de stock/inventário.
    /// Fornece endpoints CRUD completos + processamento de devoluções de alugueres e cancelamentos.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    [Authorize]
    public class StockController : BaseController
    {
        private readonly IStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;
        private readonly IStockService _stockService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Inicializa o controller de stock com dependências necessárias.
        /// </summary>
        /// <param name="stockRepository">Repositório de acesso a dados de stock</param>
        /// <param name="stockService">Serviço de lógica de negócio de stock</param>
        /// <param name="mapper">AutoMapper para conversão entre entidades e ViewModels</param>
        /// <param name="notifier">Serviço de notificações de erro</param>
        public StockController(IStockRepository stockRepository,
            IProductRepository productRepository,
            IStockService stockService,
            IMapper mapper,
            INotifierService notifier) : base(notifier)
        {
            _stockRepository = stockRepository;
            _productRepository = productRepository;
            _stockService = stockService;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtém todos os items de stock.
        /// GET: v1/stock/stock-list
        /// </summary>
        /// <returns>Lista de todos os items de stock</returns>
        [Route("stock-list")]
        public async Task<IActionResult> Index()
        {
            // Obter role do utilizador
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            if (userRole != "ADMIN")
            {
                return RedirectToAction("", "Home");
            }

            var stocks = await _stockRepository.GetAllStocksWithProduct();
            var products = await _productRepository.GetAll();

            ViewBag.Products = products;
            return View(_mapper.Map<IEnumerable<StockViewModel>>(stocks));
        }


        /// <summary>
        /// Obtém detalhes de um item de stock específico por ID.
        /// GET: v1/stock/{id}
        /// </summary>
        /// <param name="id">ID único do item de stock</param>
        /// <returns>Item de stock encontrado ou NotFound</returns>
        [Route("stock-details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var stockViewModel = await _stockRepository.GetStockWithAllDetails(id);

            if (stockViewModel == null)
                return NotFound(new { success = false, message = "Stock item not found." });

            return View(_mapper.Map<StockViewModel>(stockViewModel));
        }

        #region Creation Methods
        /// <summary>
        /// Direciona utilizador para página de criação de item de stock.
        /// GET: v1/stock/stock-create
        /// </summary>
        /// <returns>Página web</returns>
        [Route("stock-create")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            IEnumerable<Product> products;
            products = await _productRepository.GetAll();
            ViewBag.Products = products;

            return View();
        }

        /// <summary>
        /// Cria um novo item de stock.
        /// POST: v1/stock/stock-create
        /// </summary>
        /// <param name="stockViewModel">Dados do item de stock a criar</param>
        /// <returns>JSON a indicar sucesso ou erros de validação</returns>
        [Route("stock-create")]
        [HttpPost]
        public async Task<IActionResult> Create(StockViewModel stockViewModel)
        {
            try
            {
                // 1. Validation of attributes by Model State
                if (ModelStateResponse() != null) return Json(new { success = false, message = "", errors = ModelStateResponse() });

                // 2. Map ViewModel entity to Model
                var stock = _mapper.Map<Stock>(stockViewModel);

                // 3. Execute the creation order (persistence in the database)
                await _stockService.Add(stock);

                // 4. Business rules validation response in the backend (fluent Validation + DataBase)
                if (PersistenceResponse() != null) return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = MessageHelper.Generics.SuccessCreationRegistration("Stock Item", $"{stockViewModel.Ref}") });

            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion

        #region Edit Methods
        /// <summary>
        /// Direciona utilizador para página de edição de item de stock.
        /// GET: v1/stock/stock-edit/{id}
        /// </summary>
        /// <returns>Página web</returns>
        [Route("stock-edit/{id:guid}")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var stockViewModel = await GetStockDetailsById(id);
            IEnumerable<Product> products;
            products = await _productRepository.GetAll();
            ViewBag.Products = products;

            if (stockViewModel == null) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

            return View(stockViewModel);
        }

        /// <summary>
        /// Atualiza um item de stock existente.
        /// PUT: v1/stock/stock-edit/{id}
        /// </summary>
        /// <param name="id">ID do item de stock a atualizar</param>
        /// <param name="stockViewModel">Dados atualizados do item de stock</param>
        /// <returns>JSON a indicar sucesso ou erros de validação</returns>
        [Route("stock-edit/{id:guid}")]
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, StockViewModel stockViewModel)
        {
            try
            {
                // 1. Validation Id enter paramenter
                if (id != stockViewModel.Id) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

                // 2. Validation of attributes by Model State
                if (ModelStateResponse() != null) return Json(new { success = false, message = "", errors = ModelStateResponse() });

                // 3. Map ViewModel entity to Model
                var stock = _mapper.Map<Stock>(stockViewModel);

                // 4. Execute the update order (persistence in the database)
                await _stockService.Update(stock);

                // 5. Business rules validation response in the backend (fluent Validation + DataBase)
                if (PersistenceResponse() != null) return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = MessageHelper.Generics.SuccessChangeRegistration("Stock Item", $"{stockViewModel.Ref}") });
            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion

        #region Remove Methods
        /// <summary>
        /// Remove um item de stock.
        /// DELETE: v1/stock/stock-delete/{id}
        /// </summary>
        /// <param name="id">ID do item de stock a remover</param>
        /// <returns>Mensagem de sucesso ou erro se item não encontrado</returns>
        [Route("stock-delete/{id:guid}")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // 1. Get entity by Id
                var stockViewModel = await GetStockDetailsById(id);

                // 2. Validate if exist in database
                if (stockViewModel == null) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

                // 3. Execute the remove (persistence in the database)
                await _stockService.Remove(id);

                // 4. Business rules validation response in the backend (fluent Validation + DataBase)
                if (PersistenceResponse() != null) return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = MessageHelper.Generics.SuccessRemovalRegistration("Stock Item", $"{stockViewModel.Ref}") });
            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion

        #region Return Methods
        /// <summary>
        /// Processa a devolução de um aluguer (marca stock como disponível novamente).
        /// POST: v1/stock/{stockId}/return-rent
        /// </summary>
        /// <param name="stockId">ID do item de stock a devolver</param>
        /// <returns>Mensagem de sucesso ou erro se devolução falhar</returns>
        [Route("return-rent/{stockId:guid}")]
        [HttpPost]
        public async Task<IActionResult> ReturnRent(Guid stockId)
        {
            try
            {
                // 1. Get entity by Id
                var stockViewModel = await GetStockDetailsById(stockId);

                // 2. Validate if exist in database
                if (stockViewModel == null) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Stock Id") });

                // 3. Execute the rent return process
                await _stockService.ProcessRentReturn(stockId);

                // 4. Business rules validation response in the backend (fluent Validation + DataBase)
                if (PersistenceResponse() != null) return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = MessageHelper.Generics.SuccessChangeRegistration("Rent Return", $"Stock ID {stockId}") });
            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Processa o cancelamento de uma venda (devolução por cancelamento).
        /// Marca stock como disponível após cancelamento da venda.
        /// POST: v1/stock/{stockId}/return-cancel
        /// </summary>
        /// <param name="stockId">ID do item de stock cancelado</param>
        /// <returns>Mensagem de sucesso ou erro se cancelamento falhar</returns>
        [Route("return-cancel/{stockId:guid}")]
        [HttpPost]
        public async Task<IActionResult> ReturnCancel(Guid stockId)
        {
            try
            {
                // 1. Get entity by Id
                var stockViewModel = await GetStockDetailsById(stockId);

                // 2. Validate if exist in database
                if (stockViewModel == null) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Stock Id") });

                // 3. Execute the cancel return process
                await _stockService.ProcessCancelReturn(stockId);

                // 4. Business rules validation response in the backend (fluent Validation + DataBase)
                if (PersistenceResponse() != null) return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = MessageHelper.Generics.SuccessChangeRegistration("Cancelled Sale Return", $"Stock ID {stockId}") });
            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Método auxiliar privado para obter detalhes de um item de stock por ID.
        /// </summary>
        /// <param name="id">ID do item de stock</param>
        /// <returns>ViewModel do item de stock ou null se não encontrado</returns>
        private async Task<StockViewModel> GetStockDetailsById(Guid id)
        {
            return _mapper.Map<StockViewModel>(await _stockRepository.GetById(id));
        }
        #endregion
    }
}