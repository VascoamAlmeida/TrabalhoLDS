using AutoMapper;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Helper.StaticVariables;
using LojaMaquinas.WebApp.ViewModels.MVC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LojaMaquinas.WebApp.Controllers.MVC
{
    /// <summary>
    /// Controller responsável pela gestão de manutenções (MVC).
    /// Lista manutenções filtradas por role (CLIENT vê só as suas, ADMIN vê todas).
    /// </summary>
    [Authorize]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class MaintenanceController : BaseController
    {
        private readonly IMaintenanceRepository _maintenanceRepository;
        private readonly IMaintenanceService _maintenanceService;
        private readonly IOrderRepository _orderRepository;
        private readonly IClientRepository _clientRepository;
        private readonly IMapper _mapper;

        public MaintenanceController(
            IMaintenanceRepository maintenanceRepository,
            IMaintenanceService maintenanceService,
            IOrderRepository orderRepository,
            IClientRepository clientRepository,
            IMapper mapper,
            INotifierService notifier) : base(notifier)
        {
            _maintenanceRepository = maintenanceRepository;
            _maintenanceService = maintenanceService;
            _orderRepository = orderRepository;
            _clientRepository = clientRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// GET: Lista de manutenções (filtrada por role).
        /// Se CLIENT: mostra apenas manutenções associadas aos seus Orders.
        /// Se ADMIN: mostra todas as manutenções.
        /// </summary>
        [Route("maintenance-list")]
        public async Task<IActionResult> Index()
        {
            IEnumerable<Maintenance> maintenances;

            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            if (userRole == "ADMIN")
            {
                // Admin vê todas as manutenções
                maintenances = await _maintenanceRepository.GetAllMaintenancesWithOrders();
            }
            else
            {
                // Cliente vê apenas as suas manutenções
                var client = await _clientRepository.GetByUserId(userId);
                if (client == null)
                {
                    // Se não tem Client associado, não tem manutenções
                    maintenances = new List<Maintenance>();
                }
                else
                {
                    maintenances = await _maintenanceRepository.GetMaintenancesByClient(client.Id);
                }
            }

            return View(_mapper.Map<IEnumerable<MaintenanceViewModel>>(maintenances));
        }

        /// <summary>
        /// GET: Detalhes de uma manutenção (modal).
        /// </summary>
        [Route("maintenance-details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var maintenance = await _maintenanceRepository.GetMaintenanceWithDetails(id);

            if (maintenance == null)
                return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

            // Verifica se o utilizador tem permissão para ver esta manutenção
            if (!await CanAccessMaintenance(maintenance))
                return Unauthorized();

            return View(_mapper.Map<MaintenanceViewModel>(maintenance));
        }

        #region Creation Methods

        /// <summary>
        /// GET: Form para criar manutenção (modal).
        /// </summary>
        [Route("maintenance-create")]
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Carrega lista de Orders para dropdown
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            IEnumerable<Order> orders;

            if (userRole == "ADMIN")
            {
                // Admin vê todos os orders
                orders = await _orderRepository.GetAll();
            }
            else
            {
                // Cliente vê apenas os seus orders
                var client = await _clientRepository.GetByUserId(userId);
                if (client == null)
                {
                    orders = new List<Order>();
                }
                else
                {
                    orders = await _orderRepository.GetOrdersByClientId(client.Id);
                }
            }

            ViewBag.Orders = orders;
            return View();
        }

        /// <summary>
        /// POST: Cria uma nova manutenção.
        /// </summary>
        [Route("maintenance-create")]
        [HttpPost]
        public async Task<IActionResult> Create(MaintenanceViewModel maintenanceViewModel)
        {
            try
            {
                // Remove validação de Price e Status (são definidos automaticamente)
                ModelState.Remove(nameof(maintenanceViewModel.Price));
                ModelState.Remove(nameof(maintenanceViewModel.Status));

                // 1. Validação de atributos por Model State
                if (ModelStateResponse() != null)
                    return Json(new { success = false, message = "", errors = ModelStateResponse() });

                // 2. Verifica se o Order pertence ao utilizador (se CLIENT)
                var userRole = User.FindFirstValue(ClaimTypes.Role);
                if (userRole == "CLIENT")
                {
                    var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                    var client = await _clientRepository.GetByUserId(userId);
                    var order = await _orderRepository.GetById(maintenanceViewModel.OrderId);

                    if (order == null || order.ClientId != client.Id)
                    {
                        return Json(new { success = false, message = "", errors = new[] { "You can only create maintenances for your own orders." } });
                    }
                }

                // 3. Map ViewModel para Model
                var maintenance = _mapper.Map<Maintenance>(maintenanceViewModel);

                // Define valores automáticos
                maintenance.Price = 0; // Preço default
                maintenance.Status = Business.Models.Enums.MaintenanceStatus.Pending; // Sempre começa Pending

                // 4. Executa a criação
                await _maintenanceService.Add(maintenance);

                // 5. Resposta de validação de regras de negócio
                if (PersistenceResponse() != null)
                    return Json(new { success = false, message = "", errors = PersistenceResponse() });

                // 6. Resposta de sucesso
                return Json(new { success = true, message = MessageHelper.Maintenance.SuccessfullyCreated });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "", errors = new[] { ex.Message } });
            }
        }

        #endregion

        #region Edition Methods

        /// <summary>
        /// GET: Form para editar manutenção (modal).
        /// </summary>
        [Route("maintenance-edit/{id:guid}")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var maintenance = await _maintenanceRepository.GetMaintenanceWithDetails(id);

            if (maintenance == null)
                return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

            // Verifica se o utilizador tem permissão para editar esta manutenção
            if (!await CanAccessMaintenance(maintenance))
                return Unauthorized();

            // Carrega lista de Orders para dropdown
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            IEnumerable<Order> orders;

            if (userRole == "ADMIN")
            {
                orders = await _orderRepository.GetAll();
            }
            else
            {
                var client = await _clientRepository.GetByUserId(userId);
                if (client == null)
                {
                    orders = new List<Order>();
                }
                else
                {
                    orders = await _orderRepository.GetOrdersByClientId(client.Id);
                }
            }

            ViewBag.Orders = orders;
            return View(_mapper.Map<MaintenanceViewModel>(maintenance));
        }

        /// <summary>
        /// POST: Atualiza uma manutenção.
        /// </summary>
        [Route("maintenance-edit/{id:guid}")]
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, MaintenanceViewModel maintenanceViewModel)
        {
            try
            {
                if (id != maintenanceViewModel.Id)
                    return Json(new { success = false, message = "", errors = new[] { "Id mismatch" } });

                // Verifica se a manutenção existe e se o utilizador tem permissão
                var existingMaintenance = await _maintenanceRepository.GetMaintenanceWithDetails(id);
                if (existingMaintenance == null)
                    return Json(new { success = false, message = "", errors = new[] { MessageHelper.Generics.ErrorParameterNotFound("Id") } });

                if (!await CanAccessMaintenance(existingMaintenance))
                    return Unauthorized();

                // Validação de atributos
                if (ModelStateResponse() != null)
                    return Json(new { success = false, message = "", errors = ModelStateResponse() });

                // Map e atualiza
                var maintenance = _mapper.Map<Maintenance>(maintenanceViewModel);
                await _maintenanceService.Update(maintenance);

                // Resposta de validação
                if (PersistenceResponse() != null)
                    return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = MessageHelper.Maintenance.SuccessfullyUpdated });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "", errors = new[] { ex.Message } });
            }
        }

        #endregion

        #region Removal Methods

        /// <summary>
        /// POST: Remove uma manutenção.
        /// </summary>
        [Route("maintenance-remove/{id:guid}")]
        [HttpPost]
        public async Task<IActionResult> Remove(Guid id)
        {
            try
            {
                var maintenance = await _maintenanceRepository.GetMaintenanceWithDetails(id);

                if (maintenance == null)
                    return Json(new { success = false, message = "", errors = new[] { MessageHelper.Generics.ErrorParameterNotFound("Id") } });

                // Verifica se o utilizador tem permissão
                if (!await CanAccessMaintenance(maintenance))
                    return Unauthorized();

                await _maintenanceService.Remove(id);

                if (PersistenceResponse() != null)
                    return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = MessageHelper.Maintenance.SuccessfullyDeleted });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "", errors = new[] { ex.Message } });
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Verifica se o utilizador autenticado tem permissão para aceder a esta manutenção.
        /// </summary>
        private async Task<bool> CanAccessMaintenance(Maintenance maintenance)
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);

            // Admin pode aceder a todas
            if (userRole == "ADMIN")
                return true;

            // Cliente só pode aceder às suas
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var client = await _clientRepository.GetByUserId(userId);

            if (client == null)
                return false;

            var order = await _orderRepository.GetById(maintenance.OrderId);
            return order != null && order.ClientId == client.Id;
        }

        private async Task<MaintenanceViewModel> GetMaintenanceDetailsById(Guid id)
        {
            var maintenance = await _maintenanceRepository.GetMaintenanceWithDetails(id);
            return _mapper.Map<MaintenanceViewModel>(maintenance);
        }

        #endregion
    }
}
