using AutoMapper;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Interfaces.Parameters;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Business.Services.Auxiliaries;
using LojaMaquinas.Data.Repositories.Parameters;
using LojaMaquinas.Helper.StaticVariables;
using LojaMaquinas.WebApp.ViewModels.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LojaMaquinas.WebApp.Controllers.API
{
    /// <summary>
    /// API Controller para gestão de pedidos (Orders).
    /// Fornece endpoints CRUD completos + consultas, download de PDF, cancelamento e finalização de pedidos.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class OrderApiController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderService _orderService;
        private readonly PdfService _pdfService;
        private readonly IStockRepository _stockRepository;
        private readonly IProductRepository _productRepository;
        private readonly IClientRepository _clientRepository;

        /// <summary>
        /// Inicializa o controller de pedidos com dependências necessárias.
        /// </summary>
        /// <param name="mapper">AutoMapper para conversão entre entidades e ViewModels</param>
        /// <param name="notifier">Serviço de notificações de erro</param>
        /// <param name="orderRepository">Repositório de acesso a dados de pedidos</param>
        /// <param name="orderService">Serviço de lógica de negócio de pedidos</param>
        /// <param name="pdfService">Serviço para geração de PDFs de pedidos</param>
        /// <param name="stockRepository">Repositório de stock para consulta de produtos</param>
        /// <param name="productRepository">Repositório de produtos</param>
        public OrderApiController(
            IMapper mapper,
            INotifierService notifier, IOrderRepository orderRepository,
            IOrderService orderService, PdfService pdfService,
            IStockRepository stockRepository, IProductRepository productRepository,
            IClientRepository clientRepository
        ) : base(notifier)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _orderService = orderService;
            _pdfService = pdfService;
            _stockRepository = stockRepository;
            _productRepository = productRepository;
            _clientRepository = clientRepository;
        }

        /// <summary>
        /// Verifica se o utilizador autenticado pode aceder aos dados de um cliente.
        /// Administradores podem aceder a qualquer cliente; clientes apenas ao próprio perfil.
        /// </summary>
        private async Task<bool> CanAccessClient(Guid clientId)
        {
            if (IsAdmin)
                return true;

            if (!AuthenticatedUserId.HasValue)
                return false;

            var client = await _clientRepository.GetById(clientId);
            return client != null && client.UserId == AuthenticatedUserId.Value;
        }

        #region Get Methods
        /// <summary>
        /// Obtém todos os pedidos.
        /// GET: api/v1/orderapi
        /// </summary>
        /// <returns>Lista de todos os pedidos</returns>
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAll()
        {
            var orders = await _orderRepository.GetAll();
            var viewModels = _mapper.Map<IEnumerable<OrderApiViewModel>>(orders);
            return Ok(viewModels);
        }

        /// <summary>
        /// Obtém um pedido específico por ID.
        /// GET: api/v1/orderapi/{id}
        /// </summary>
        /// <param name="id">ID único do pedido</param>
        /// <returns>Pedido encontrado ou NotFound</returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var order = await _orderRepository.GetById(id);
            if (order == null)
                return NotFound(new { success = false, message = "Order not found." });

            if (!await CanAccessClient(order.ClientId))
                return Forbid();

            var viewModel = _mapper.Map<OrderApiViewModel>(order);
            return Ok(viewModel);
        }

        /// <summary>
        /// Obtém todos os pedidos de um cliente específico.
        /// GET: /client/{id}
        /// </summary>
        /// <param name="id">ID do cliente</param>
        /// <returns>Lista de pedidos do cliente</returns>
        [HttpGet("client/{id:guid}")]
        public async Task<IActionResult> GetByClientId(Guid id)
        {
            if (!await CanAccessClient(id))
                return Forbid();

            var order = await _orderRepository.GetOrdersByClientId(id);
            if (order == null)
                return NotFound(new { success = false, message = "Order not found." });

            var viewModel = _mapper.Map<IEnumerable<OrderApiViewModel>> (order);
            return Ok(viewModel);
        }

        /// <summary>
        /// Obtém os dados completos de um pedido para checkout (inclui cliente e stock).
        /// GET: api/v1/orderapi/{id}/checkout
        /// </summary>
        /// <param name="id">ID do pedido</param>
        /// <returns>Dados completos do pedido ou NotFound</returns>
        [HttpGet("{id:guid}/checkout")]
        public async Task<IActionResult> Checkout(Guid id)
        {
            var order = await _orderRepository.GetOrderAndClientAndStockByOrderId(id);
            if (order == null)
                return NotFound(new { success = false, message = MessageHelper.Generics.ErrorParameterNotFound("Id") });

            if (!await CanAccessClient(order.ClientId))
                return Forbid();

            var viewModel = _mapper.Map<OrderApiViewModel>(order);
            return Ok(viewModel);
        }

        /// <summary>
        /// Gera e faz download de um PDF de um pedido (com dados de cliente, stock e produto).
        /// GET: api/v1/orderapi/{id}/download-pdf
        /// </summary>
        /// <param name="id">ID do pedido</param>
        /// <returns>Ficheiro PDF do pedido</returns>
        [HttpGet("{id:guid}/download-pdf")]
        public async Task<IActionResult> DownloadOrderPdf(Guid id)
        {
            try
            {
                var order = await _orderRepository.GetById(id);

                if (order == null)
                    return NotFound(new { success = false, message = "Order not found." });

                if (!await CanAccessClient(order.ClientId))
                    return Forbid();

                var client = await _clientRepository.GetById(order.ClientId);
                if (client == null)
                    return NotFound(new { success = false, message = "Client not found for this order." });

                var stock = await _stockRepository.GetById(order.StockId);
                if (stock == null)
                    return NotFound(new { success = false, message = "Stock item not found for this order." });

                var product = await _productRepository.GetById(stock.ProductId);
                if (product == null)
                    return NotFound(new { success = false, message = "Product not found for this order." });

                var pdfBytes = _pdfService.GenerateOrderPdf(order, client, stock, product);

                var shortId = order.Id.ToString()[..8];

                return File(pdfBytes, "application/pdf", $"Order_{shortId}.pdf");
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, error = "An unexpected error occurred." });
            }
        }
        #endregion

        #region Create Methods
        /// <summary>
        /// Cria um novo pedido.
        /// POST: api/v1/orderapi
        /// </summary>
        /// <param name="orderViewModel">Dados do pedido a criar</param>
        /// <returns>Mensagem de sucesso ou erros de validação</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderApiViewModel orderViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!await CanAccessClient(orderViewModel.ClientId))
                    return Forbid();

                if (!IsAdmin)
                {
                    orderViewModel.Status = OrderStatus.Pending;

                    if (orderViewModel.Type == OrderType.Sale)
                    {
                        var stock = await _stockRepository.GetById(orderViewModel.StockId);
                        if (stock == null)
                            return NotFound(new { success = false, message = "Stock item not found." });

                        if (stock.StockStatus != StockStatus.Available)
                            return BadRequest(new { success = false, message = "Stock item is not available." });

                        var product = await _productRepository.GetById(stock.ProductId);
                        if (product == null)
                            return NotFound(new { success = false, message = "Product not found." });

                        orderViewModel.Total = product.Price;
                    }
                }

                var order = _mapper.Map<Order>(orderViewModel);
                await _orderService.Add(order);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    orderId = order.Id,
                    message = MessageHelper.Generics.SuccessCreationRegistration("Order", orderViewModel.Id.ToString())
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, error = "An unexpected error occurred." });
            }
        }
        #endregion

        #region Update Methods
        /// <summary>
        /// Atualiza um pedido existente.
        /// PUT: api/v1/orderapi/{id}
        /// </summary>
        /// <param name="id">ID do pedido a atualizar</param>
        /// <param name="orderViewModel">Dados atualizados do pedido</param>
        /// <returns>Mensagem de sucesso ou erros de validação</returns>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrderApiViewModel orderViewModel)
        {
            if (id != orderViewModel.Id)
                return BadRequest(new { success = false, message = "Order Id invalid." });

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var order = _mapper.Map<Order>(orderViewModel);
                await _orderService.Update(order);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });
                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessChangeRegistration("Order", orderViewModel.Id.ToString())
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, error = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Cancela um pedido (altera status para Cancelled).
        /// Não permite cancelar pedidos já completados.
        /// PATCH: api/v1/orderapi/{id}/cancel
        /// </summary>
        /// <param name="id">ID do pedido a cancelar</param>
        /// <returns>Mensagem de sucesso ou erro</returns>
        [HttpPatch("{id:guid}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            try
            {
                var existingOrder = await _orderRepository.GetById(id);

                if (existingOrder == null)
                    return NotFound(new { success = false, message = "Order not found." });

                if (!await CanAccessClient(existingOrder.ClientId))
                    return Forbid();

                if (existingOrder.Status == OrderStatus.Completed)
                    return BadRequest(new { success = false, message = "Cannot cancel a completed order." });

                if (existingOrder.Status == OrderStatus.Cancelled)
                    return BadRequest(new { success = false, message = "Order is already cancelled." });

                existingOrder.Status = OrderStatus.Cancelled;
                await _orderService.Update(existingOrder);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = "Order cancelled successfully."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, error = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Marca um pedido como ativo (altera status para Active).
        /// PATCH: api/v1/orderapi/{id}/active
        /// </summary>
        /// <param name="id">ID do pedido a ativar</param>
        /// <returns>Mensagem de sucesso ou erro</returns>
        [HttpPatch("{id:guid}/active")]
        public async Task<IActionResult> ActiveOrder(Guid id)
        {
            try
            {
                var existingOrder = await _orderRepository.GetById(id);

                if (existingOrder == null)
                    return NotFound(new { success = false, message = "Order not found." });

                if (!await CanAccessClient(existingOrder.ClientId))
                    return Forbid();

                if (existingOrder.Status != OrderStatus.Pending)
                    return BadRequest(new { success = false, message = "Only pending orders can be activated." });

                existingOrder.Status = OrderStatus.Active;
                existingOrder.Total *= 1.23m;

                await _orderService.Update(existingOrder);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = "Order activated successfully."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, error = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Marca um pedido como completo (altera status para Completed).
        /// Não permite completar pedidos já cancelados.
        /// PATCH: api/v1/orderapi/{id}/complete
        /// </summary>
        /// <param name="id">ID do pedido a completar</param>
        /// <returns>Mensagem de sucesso ou erro</returns>
        [HttpPatch("{id:guid}/complete")]
        public async Task<IActionResult> CompleteOrder(Guid id)
        {
            try
            {
                var existingOrder = await _orderRepository.GetById(id);

                if (existingOrder == null)
                    return NotFound(new { success = false, message = "Order not found." });

                if (!await CanAccessClient(existingOrder.ClientId))
                    return Forbid();

                if (existingOrder.Status != OrderStatus.Active)
                    return BadRequest(new { success = false, message = "Only active orders can be completed." });

                existingOrder.Status = OrderStatus.Completed;
                await _orderService.Update(existingOrder);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = "Order completed successfully."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, error = "An unexpected error occurred." });
            }
        }
        #endregion

        #region Delete Method
        /// <summary>
        /// Remove um pedido permanentemente.
        /// DELETE: api/v1/orderapi/{id}
        /// </summary>
        /// <param name="id">ID do pedido a remover</param>
        /// <returns>Mensagem de sucesso ou erro se pedido não encontrado</returns>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var order = await _orderRepository.GetById(id);
                if (order == null)
                    return NotFound(new { success = false, message = "Order not found." });

                await _orderService.Remove(id);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessRemovalRegistration("Order", order.Id.ToString())
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, error = "An unexpected error occurred." });
            }
        }
        #endregion
    }
}
