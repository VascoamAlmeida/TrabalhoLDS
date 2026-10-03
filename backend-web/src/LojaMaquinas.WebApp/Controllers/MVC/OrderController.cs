using AutoMapper;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Interfaces.Parameters;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Business.Services.Auxiliaries;
using LojaMaquinas.Data.Repositories.Entities;
using LojaMaquinas.Helper.StaticVariables;
using LojaMaquinas.WebApp.ViewModels.API;
using LojaMaquinas.WebApp.ViewModels.MVC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LojaMaquinas.WebApp.Controllers.MVC
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class OrderController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;
        private readonly PdfService _pdfService;
        private readonly IStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IOrderRentRepository _orderRentRepository;

        public OrderController(IMapper mapper,
            INotifierService notifier, IOrderRepository orderRepository,
            IOrderService orderService, PdfService pdfService,
            IStockRepository stockRepository, IProductRepository productRepository, 
            IClientRepository clientRepository, IOrderRentRepository orderRentRepository) : base(notifier)
            
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _orderService = orderService;
            _pdfService = pdfService;
            _stockRepository = stockRepository;
            _productRepository = productRepository;
            _clientRepository = clientRepository;
            _orderRentRepository = orderRentRepository;
        }

        /// <summary>
        /// Verifica se o utilizador autenticado pode aceder ao pedido indicado.
        /// Administradores podem aceder a qualquer pedido; clientes apenas aos próprios pedidos.
        /// </summary>
        private async Task<bool> CanAccessOrder(Order order)
        {
            if (User.IsInRole("ADMIN"))
                return true;

            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdValue, out var userId))
                return false;

            var client = await _clientRepository.GetById(order.ClientId);
            return client != null && client.UserId == userId;
        }

        #region GET Methods
        [Route("order-list")]
        public async Task<IActionResult> Index()
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            IEnumerable<Order> orders;

            if (userRole == "ADMIN")
            {
                orders = await _orderRepository.GetOrdersAndClientAndStock();
            }
            else
            {
                var client = await _clientRepository.GetByUserId(userId);

                if (client == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                orders = await _orderRepository.GetOrdersAndStockByClientId(client.Id);
            }

            var viewModels = _mapper.Map<IEnumerable<OrderViewModel>>(orders);
            return View(viewModels);
        }

        [Route("order-details/{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var order = await _orderRepository.GetOrderAndClientAndStockByOrderId(id);
            if (order == null)
                return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

            if (!await CanAccessOrder(order))
                return Forbid();

            var viewModel = _mapper.Map<OrderViewModel>(order);
            return View("Details", viewModel);
        }

        [Route("order-checkout/{id:guid}")]
        public async Task<IActionResult> Checkout(Guid id)
        {
            var order = await _orderRepository.GetOrderAndClientAndStockByOrderId(id);
            if (order == null)
                return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

            if (!await CanAccessOrder(order))
                return Forbid();

            var viewModel = _mapper.Map<OrderViewModel>(order);
            return View(viewModel);
        }
        #endregion

        #region PDF Method
        [HttpGet("{id:guid}/download-pdf")]
        public async Task<IActionResult> DownloadOrderPdf(Guid id)
        {
            try
            {
                var order = await _orderRepository.GetById(id);

                if (order == null)
                    return NotFound(new { success = false, message = "Order not found." });

                if (!await CanAccessOrder(order))
                    return Forbid();

                var orderRent = await _orderRepository.GetOrderRentByOrderId(id);
                var client = await _clientRepository.GetById(order.ClientId);
                var stock = await _stockRepository.GetById(order.StockId);
                var product = await _productRepository.GetById(stock.ProductId);

                if (order.Type == 0)
                {
                    var pdfBytes = _pdfService.GenerateOrderPdf(order, client, stock, product);
                    var shortId = order.Id.ToString()[..8];
                    return File(pdfBytes, "application/pdf", $"Order_{shortId}.pdf");
                }
                else
                {
                    var pdfBytes = _pdfService.GenerateOrderRentPdf(order, client, stock, product, orderRent);
                    var shortId = order.Id.ToString()[..8];
                    return File(pdfBytes, "application/pdf", $"OrderRent_{shortId}.pdf");
                }
                

                
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }
        #endregion

        #region ADD Methods
        [HttpGet]
        [Route("order-create/{stockId:guid}")]
        public async Task<IActionResult> Create(Guid stockId)
        {
            // Obter a Stock + Produto associado
            var stock = await _stockRepository.GetStockWithAllDetails(stockId);
            if (stock == null)
                return RedirectToAction("Index", "Home");

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var client = await _clientRepository.GetByUserId(userId);
            if (client == null)
                return RedirectToAction("Index", "Home");

            // Preencher o ViewModel com dados base
            var viewModel = new OrderViewModel
            {
                StockId = stockId,
                ClientId = client.Id,
                Stock = stock,
                Total = stock.Product.Price
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("order-create/{stockId:guid}")]
        public async Task<IActionResult> Create(Guid stockId, OrderViewModel orderViewModel)
        {
            try
            {
                // Garantir StockId, cliente autenticado e valores calculados no servidor.
                var stock = await _stockRepository.GetStockWithAllDetails(stockId);
                if (stock == null)
                    return Json(new { success = false, error = "Stock não encontrado." });

                var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdValue, out var userId))
                    return Forbid();

                var client = await _clientRepository.GetByUserId(userId);
                if (client == null)
                    return Json(new { success = false, error = "Perfil de cliente não encontrado." });

                orderViewModel.StockId = stockId;
                orderViewModel.ClientId = client.Id;
                orderViewModel.Total = stock.Product.Price;
                orderViewModel.OrderDate = DateTime.Now;
                orderViewModel.Status = OrderStatus.Pending;

                if (!ModelState.IsValid)
                    return Json(new { success = false, errors = ModelStateResponse() });

                var order = _mapper.Map<Order>(orderViewModel);

                await _orderRepository.Add(order);
                await _orderRepository.SaveChanges();

                var persistenceErrors = PersistenceResponse();
                if (persistenceErrors != null)
                    return Json(new { success = false, message = "", errors = persistenceErrors });

                return Json(new { success = true, message = "O Pedido foi criado com sucesso!", orderId = order.Id });
            }
            catch (Exception ex)
            {
                var fullError = $"Erro: {ex.Message} | InnerException: {ex.InnerException?.Message}";
                return Json(new { success = false, message = "", errors = new[] { fullError } });
            }
        }
        #endregion

        #region EDIT Methods
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        [Route("order-edit/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var order = await _orderRepository.GetOrderAndClientAndStockByOrderId(id);

            if (order == null)
                return NotFound();

            var viewModel = _mapper.Map<OrderViewModel>(order);
            return View(viewModel);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        [Route("order-edit/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id, OrderViewModel orderViewModel)
        {
            try
            {
                // 1. Validar se o ID corresponde
                if (id != orderViewModel.Id)
                    return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

                // 2. Validar ModelState
                var modelErrors = ModelStateResponse();
                if (modelErrors != null)
                    return Json(new { success = false, message = "", errors = modelErrors });

                // 3. Obter a order existente
                var existingOrder = await _orderRepository.GetById(id);
                if (existingOrder == null)
                    return Json(new { success = false, error = "Pedido não encontrado!" });

                // 4. Atualizar os campos
                existingOrder.ClientId = orderViewModel.ClientId;
                existingOrder.StockId = orderViewModel.StockId;
                existingOrder.OrderDate = orderViewModel.OrderDate;
                existingOrder.Total = orderViewModel.Total;
                existingOrder.Type = orderViewModel.Type;
                existingOrder.Status = orderViewModel.Status;

                // 5. Guardar alterações
                await _orderRepository.Update(existingOrder);
                await _orderRepository.SaveChanges();

                // 6. Verificar se houve erros de persistência
                var persistenceErrors = PersistenceResponse();
                if (persistenceErrors != null)
                    return Json(new { success = false, message = "", errors = persistenceErrors });

                return Json(new { success = true, message = "O Pedido foi atualizado com sucesso!" });
            }
            catch (Exception ex)
            {
                var fullError = $"Erro: {ex.Message} | InnerException: {ex.InnerException?.Message}";
                return Json(new { success = false, message = "", errors = new[] { fullError } });
            }
        }
        #endregion

        #region PATCH Methods
        /// <summary>
        /// Cancela um pedido (altera status para Cancelled).
        /// Não permite cancelar pedidos já completados.
        /// </summary>
        /// <param name="id">ID do pedido a cancelar</param>
        /// <returns>Mensagem de sucesso ou erro</returns>
        [HttpPatch]
        [Route("order-cancel/{id:guid}")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            try
            {
                var existingOrder = await _orderRepository.GetById(id);

                if (existingOrder == null)
                    return Json(new { success = false, error = "Pedido não encontrado!" });

                if (!await CanAccessOrder(existingOrder))
                    return Forbid();

                // Validar se a ordem pode ser cancelada
                if (existingOrder.Status == OrderStatus.Completed)
                    return Json(new { success = false, error = "Não é possível cancelar um pedido concluído!" });

                if (existingOrder.Status == OrderStatus.Cancelled)
                    return Json(new { success = false, error = "O pedido já está cancelado!" });

                existingOrder.Status = OrderStatus.Cancelled;
                await _orderRepository.Update(existingOrder);
                await _orderRepository.SaveChanges();

                var persistenceErrors = PersistenceResponse();
                if (persistenceErrors != null)
                    return Json(new { success = false, message = "", errors = persistenceErrors });

                return Json(new { success = true, message = "O Pedido foi cancelado com sucesso!" });
            }
            catch (Exception ex)
            {
                var fullError = $"Erro: {ex.Message} | InnerException: {ex.InnerException?.Message}";
                return Json(new { success = false, message = "", errors = new[] { fullError } });
            }
        }

        /// <summary>
        /// Marca um pedido como ativo (altera status para Active).
        /// Não permite ativar pedidos já cancelados.
        /// </summary>
        /// <param name="id">ID do pedido a ativo</param>
        /// <returns>Mensagem de sucesso ou erro</returns>
        [HttpPatch]
        [Route("order-active/{id:guid}")]
        public async Task<IActionResult> ActiveOrder(Guid id)
        {
            try
            {
                var existingOrder = await _orderRepository.GetById(id);

                if (existingOrder == null)
                    return Json(new { success = false, error = "Pedido não encontrado!" });

                if (!await CanAccessOrder(existingOrder))
                    return Forbid();

                if (existingOrder.Status != OrderStatus.Pending)
                    return Json(new { success = false, error = "Apenas pedidos pendentes podem ser ativados." });

                // Obter o stock e produto para calcular o total
                var stock = await _stockRepository.GetById(existingOrder.StockId);
                if (stock != null)
                {
                    var product = await _productRepository.GetById(stock.ProductId);
                    if (product != null)
                    {
                        // Calcular o total com IVA (23%)
                        existingOrder.Total = product.Price * 1.23m;
                    }
                }

                existingOrder.Status = OrderStatus.Active;
                await _orderRepository.Update(existingOrder);
                await _orderRepository.SaveChanges();

                var persistenceErrors = PersistenceResponse();
                if (persistenceErrors != null)
                    return Json(new { success = false, message = "", errors = persistenceErrors });

                return Json(new { success = true, message = "O Pedido foi ativado com sucesso!" });
            }
            catch (Exception ex)
            {
                var fullError = $"Erro: {ex.Message} | InnerException: {ex.InnerException?.Message}";
                return Json(new { success = false, message = "", errors = new[] { fullError } });
            }
        }

        /// <summary>
        /// Marca um pedido como completo (altera status para Completed).
        /// Não permite completar pedidos já cancelados.
        /// </summary>
        /// <param name="id">ID do pedido a completar</param>
        /// <returns>Mensagem de sucesso ou erro</returns>
        [HttpPatch]
        [Authorize(Roles = "ADMIN")]
        [Route("order-complete/{id:guid}")]
        public async Task<IActionResult> CompleteOrder(Guid id)
        {
            try
            {
                var existingOrder = await _orderRepository.GetById(id);

                if (existingOrder == null)
                    return Json(new { success = false, error = "Pedido não encontrado!" });

                if (existingOrder.Status != OrderStatus.Active)
                    return Json(new { success = false, error = "Apenas pedidos ativos podem ser concluídos." });

                existingOrder.Status = OrderStatus.Completed;
                await _orderRepository.Update(existingOrder);
                await _orderRepository.SaveChanges();

                var persistenceErrors = PersistenceResponse();
                if (persistenceErrors != null)
                    return Json(new { success = false, message = "", errors = persistenceErrors });

                return Json(new { success = true, message = "O Pedido foi concluído com sucesso!" });
            }
            catch (Exception ex)
            {
                var fullError = $"Erro: {ex.Message} | InnerException: {ex.InnerException?.Message}";
                return Json(new { success = false, message = "", errors = new[] { fullError } });
            }
        }
        #endregion

        #region DELETE Methods
        [HttpDelete]
        [Route("order-delete/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var order = await _orderRepository.GetById(id);
                if (order == null)
                    return Json(new { success = false, error = "Pedido não encontrado!" });

                if (!await CanAccessOrder(order))
                    return Forbid();

                // Remover OrderRent se existir
                var orderRent = await _orderRepository.GetOrderRentByOrderId(id);
                if (orderRent != null)
                {
                    // Remover Contract se existir
                    var contract = await _orderRentRepository.GetContractByOrderRentId(orderRent.Id);
                    if (contract != null)
                    {
                        await _orderRentRepository.DeleteContractByOrderRent(contract);
                    }

                    // Remover OrderRent
                    await _orderRepository.RemoveOrderRentByOrder(orderRent);
                }

                // Remover a order
                await _orderRepository.Remove(id);
                await _orderRepository.SaveChanges();

                var persistenceErrors = PersistenceResponse();
                if (persistenceErrors != null)
                    return Json(new { success = false, message = "", errors = persistenceErrors });

                return Json(new { success = true, message = "O Pedido foi eliminado com sucesso!" });
            }
            catch (Exception ex)
            {
                var fullError = $"Erro: {ex.Message} | InnerException: {ex.InnerException?.Message}";
                return Json(new { success = false, message = "", errors = new[] { fullError } });
            }
        }
        #endregion

        #region PRIVATE Methods
        //private async Task<OrderViewModel> GetOrderAndClientAndStockByOrderId(Guid id)
        //{
        //    var order = _mapper.Map<OrderViewModel>(await _orderRepository.GetOrderAndClientAndStockByOrderId(id));
        //    return order;
        //}
        #endregion
    }
}
