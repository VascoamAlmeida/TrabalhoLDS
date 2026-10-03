using AutoMapper;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Interfaces.Parameters;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.WebApp.ViewModels.API;
using LojaMaquinas.WebApp.ViewModels.MVC.OrderRent;
using Microsoft.AspNetCore.Mvc;

namespace LojaMaquinas.WebApp.Controllers.MVC
{
    /// <summary>
    /// Controller da criação de pedidos de OrderRent
    /// </summary>
    public class OrderRentController : Controller
    {

        private readonly IOrderRentRepository _orderRentRepository;
        private readonly IProductRepository _productRepository;
        private readonly IOrderRentService _orderRentService;
        private readonly IOrderRepository _orderRepository;
        private readonly INotifierService _notifier;
        private readonly IMapper _mapper;

        /// <summary>
        /// Inicializa o controller de contratos com dependências necessárias.
        /// </summary>
        /// <param name="mapper">AutoMapper para conversão entre entidades e ViewModels</param>
        /// <param name="orderRentRepository">Repositorio da OrderRent</param>
        /// <param name="productRepository">Repositorio do produto</param>
        /// <param name="orderRentService">Serviço da OrderRent</param>
        /// <param name="orderRepository">Repositorio de Order</param>
        /// <param name="notifier">Utilizar notificação</param>
        public OrderRentController(IMapper mapper,
                                  IOrderRentRepository orderRentRepository,
                                  IProductRepository productRepository,
                                  IOrderRentService orderRentService,
                                  IOrderRepository orderRepository,
                                  INotifierService notifier
                                )
        {
            _mapper = mapper;
            _productRepository = productRepository;
            _orderRentRepository = orderRentRepository;
            _orderRentService = orderRentService;
            _orderRepository = orderRepository;
            _notifier = notifier;
        }

        /// <summary>
        /// Mostra formulário de criação de um pedido de orderRent
        /// </summary>
        /// <param name="orderId">Id da order para aluguer</param>
        /// <returns>O formulario</returns>
        [Route("orderrent-create/{id:guid}")]
        [HttpGet]
        public async Task<IActionResult> Create(Guid id)
        {
            var order = await _orderRepository.GetOrderAndClientAndStockByOrderId(id);
            if (order == null)
            {
                return NotFound();
            }

            var rentOrder = new CreateOrderRentViewModel
            {
                OrderId = id,
                MachineId = order.Stock.ProductId,
                MachineName = order.Stock.Product.Name,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1),
                Total = order.Total
            };

            return View(rentOrder);
        }

        /// <summary>
        /// Pagina para criar a orderRent
        /// </summary>
        /// <param name="createOrderRentViewModel"> dados do utilizador da OrderRent</param>
        /// <returns>Formulario da pagina criar orderRent </returns>
        [HttpPost]
        [Route("orderrent-create/{id:guid}")]
        public async Task<IActionResult> Create(CreateOrderRentViewModel createOrderRentViewModel)
        {

            // Validar modelo
            if (!ModelState.IsValid)
            {
                return View(createOrderRentViewModel);
            }

            // Validar datas
            if (createOrderRentViewModel.StartDate.Date >= createOrderRentViewModel.EndDate.Date)
            {
                ModelState.AddModelError(string.Empty, "The start thats needs to be before the end date");

                return View(createOrderRentViewModel);
            }

            // Valida existencia foto
            if (createOrderRentViewModel.CompanyPhoto == null || createOrderRentViewModel.CompanyPhoto.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "No company photo");

                return View(createOrderRentViewModel);
            }

            // Guarda ficheiro 
            var folderPath = Path.Combine("wwwroot", "images", "orderRent");
            Directory.CreateDirectory(folderPath); //garante que pasta existe

            // Cria o nome do foto
            var fileName = Guid.NewGuid() + Path.GetExtension(createOrderRentViewModel.CompanyPhoto.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            // Guarda o foto
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await createOrderRentViewModel.CompanyPhoto.CopyToAsync(stream);
            }

            var orderRent = _mapper.Map<OrderRent>(createOrderRentViewModel);

            orderRent.CompanyImage = "/images/orderRent/" + fileName;


            await _orderRentService.Add(orderRent);

            if (_notifier.HasNotification())
            {
                LoadNotificationsToModelState();
                return View(createOrderRentViewModel);
            }

            return RedirectToAction(
                    actionName: "Get",
                    controllerName: "Contract",
                    routeValues: new { orderRentId = orderRent.Id }
                    );

        }

        /// <summary>
        /// Pagina de sucesso
        /// </summary>
        /// <param name="valid">Mensagem </param>
        /// <returns>Pagina de sucesso</returns>
        [HttpGet]
        [Route("orderRent-success")]
        public IActionResult Success()
        {
            {
                return View();
            }
        }

        /// <summary>
        /// Page para apresentar a lista de todas as OrderRent
        /// </summary>
        /// <returns>Todas as OrderRents existentes</returns>
        [HttpGet]
        [Route("orderRent-list")]
        public async Task<IActionResult> Index()
        {
            var orderRents = await _orderRentRepository.GetAll();

            var result = _mapper.Map<IEnumerable<OrderRentViewModel>>(orderRents);

            return View(result);
        }

        /// <summary>
        /// Page para apresentar uma orderRent especifica
        /// </summary>
        /// <param name="id">Id da OrderRent que se pretende ver</param>
        /// <returns>Apresenta os detalhes de OrderRent especifica</returns>
        [HttpGet]
        [Route("orderRent-details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var orderRents = await _orderRentRepository.GetOrderRentAndOrderAndContractByOrderRentId(id);

            if (orderRents == null)
            {
                return NotFound();
            }

            var result = _mapper.Map<OrderRentViewModel>(orderRents);

            return View(result);

        }

        /// <summary>
        /// Mostra formulario de edição de OrderRent especifico
        /// </summary>
        /// <param name="id">Id do OrderRent</param>
        /// <returns>Formulario preenchido</returns>
        [HttpGet]
        [Route("orderRent-edit/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var orderRents = await _orderRentRepository.GetById(id);

            if (orderRents == null)
            {
                return NotFound();
            }

            var result = _mapper.Map<OrderRentViewModel>(orderRents);

            return View(result);
        }

        /// <summary>
        /// Recebe o formulario para editar da OrderRent e atualiza
        /// </summary>
        /// <param name="id">Id do OrderRent</param>
        /// <param name="editOrderRent">Dados editados</param>
        /// <param name="userPhoto">Foto de utilizador</param>
        /// <returns>Volta a lista ou da erro</returns>
        [HttpPost]
        [Route("orderRent-edit/{id:guid}")]
        public async Task<IActionResult> Edit(Guid id, OrderRentViewModel editOrderRent, IFormFile? userPhoto)
        {
            if (id != editOrderRent.Id)
            {
                return NotFound();
            }

            // Validar modelo
            if (!ModelState.IsValid)
            {
                return View(editOrderRent);
            }

            var result = _mapper.Map<OrderRent>(editOrderRent);

            // Se mudar status para completed, validar e guardar foto do utilizador
            if (editOrderRent.Status == OrderRentStatus.Completed)
            {
                if (userPhoto == null || userPhoto.Length == 0)
                {
                    ModelState.AddModelError(string.Empty, "No user photo");

                    return View(editOrderRent);
                }

                // Guarda foto utilizador 
                var folderPath = Path.Combine("wwwroot", "images", "orderRent", "users");
                Directory.CreateDirectory(folderPath); //garante que pasta existe

                // Cria o nome do foto
                var fileName = Guid.NewGuid() + Path.GetExtension(userPhoto.FileName);
                var filePath = Path.Combine(folderPath, fileName);

                // Guarda o foto
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await userPhoto.CopyToAsync(stream);
                }
                result.UserImage = "/images/orderRent/users/" + fileName;

            }

            await _orderRentService.Update(result);

            if (_notifier.HasNotification())
            {
                LoadNotificationsToModelState();
                return View(editOrderRent);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Page de sucess da remoçao OrderRent
        /// </summary>
        /// <param name="id">Id da OrderRent remover</param>
        /// <returns>Apresenta confirmaçao</returns>
        [HttpGet]
        [Route("orderRent-delete/{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var orderRents = await _orderRentRepository.GetById(id);

            if (orderRents == null)
            {
                return NotFound();
            }

            var result = _mapper.Map<OrderRentViewModel>(orderRents);

            return View(result);
        }

        /// <summary>
        /// Recebe confirmaçao e tenta remover OrderRent, só remove se for Cancelled
        /// </summary>
        /// <param name="id">Id da OrderRent</param>
        /// <returns>Volta para a lista ou dá erro</returns>
        [HttpPost, ActionName("Delete")]
        [Route("orderRent-delete/{id:guid}")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {

            await _orderRentService.Remove(id);

            if (_notifier.HasNotification())
            {
                // não conseguiu remover (não está Cancelled, etc.)
                LoadNotificationsToModelState();

                var orderRent = await _orderRentRepository.GetById(id);
                if (orderRent == null)
                {
                    return RedirectToAction("Index");
                }

                var vm = _mapper.Map<OrderRentViewModel>(orderRent);
                return View(vm);
            }

            return RedirectToAction("Index");
        }

        /// <summary>
        /// Devolve todos os alugueres cujo contrato termina
        /// nos próximos N dias (alerta fim de contrato).
        /// GET: api/v1/orderrent/ending-soon?days=7
        /// </summary>
        /// <param name="days">
        /// Número de dias a partir de hoje. Por defeito 7.
        /// </param>
        [HttpGet("ending-soon")]
        public async Task<IActionResult> GetContractsEndingSoon([FromQuery] int days = 7)
        {
            if (days <= 0)
                return BadRequest(new { success = false, message = "Tem ser maior que zero" });

            var rents = await _orderRentService.GetRentsEndingSoon(days);

            var vm = _mapper.Map<IEnumerable<OrderRentApiViewModel>>(rents);

            if (!vm.Any())
                return NotFound(new { success = false, message = $"Não existem contratos a terminar nos próximos {days} dias." });

            return Ok(new
            {
                success = true,
                data = vm,
                message = $"Foram encontrados {vm.Count()} contrato(s) a terminar nos próximos {days} dia(s)."
            });
        }

        private void LoadNotificationsToModelState()
        {
            var notifications = _notifier.GetNotification();
            foreach (var n in notifications)
            {
                ModelState.AddModelError(string.Empty, n.Message);
            }
        }

    }
}
