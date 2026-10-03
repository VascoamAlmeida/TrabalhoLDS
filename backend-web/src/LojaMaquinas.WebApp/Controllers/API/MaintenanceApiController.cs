using AutoMapper;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Helper.StaticVariables;
using LojaMaquinas.WebApp.ViewModels.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LojaMaquinas.WebApp.Controllers.API
{
    /// <summary>
    /// API Controller para gestão de manutenções.
    /// Fornece endpoints CRUD completos + consultas por pedido e por cliente.
    /// </summary>
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    [ApiController]
    public class MaintenanceApiController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IMaintenanceRepository _maintenanceRepository;
        private readonly IMaintenanceService _maintenanceService;

        /// <summary>
        /// Inicializa o controller de manutenções com dependências necessárias.
        /// </summary>
        /// <param name="mapper">AutoMapper para conversão entre entidades e ViewModels</param>
        /// <param name="notifier">Serviço de notificações de erro</param>
        /// <param name="maintenanceRepository">Repositório de acesso a dados de manutenções</param>
        /// <param name="maintenanceService">Serviço de lógica de negócio de manutenções</param>
        public MaintenanceApiController(
            IMapper mapper,
            INotifierService notifier,
            IMaintenanceRepository maintenanceRepository,
            IMaintenanceService maintenanceService
        ) : base(notifier)
        {
            _mapper = mapper;
            _maintenanceRepository = maintenanceRepository;
            _maintenanceService = maintenanceService;
        }

        /// <summary>
        /// Obtém todas as manutenções.
        /// GET: api/maintenanceapi
        /// </summary>
        /// <returns>Lista de todas as manutenções</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var maintenances = await _maintenanceRepository.GetAll();
            var viewModels = _mapper.Map<IEnumerable<MaintenanceApiViewModel>>(maintenances);
            return Ok(viewModels);
        }

        /// <summary>
        /// Obtém uma manutenção específica por ID.
        /// GET: api/maintenanceapi/{id}
        /// </summary>
        /// <param name="id">ID único da manutenção</param>
        /// <returns>Manutenção encontrada ou NotFound</returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var maintenance = await _maintenanceRepository.GetById(id);
            if (maintenance == null)
                return NotFound(new { success = false, message = "Maintenance not found." });

            var viewModel = _mapper.Map<MaintenanceApiViewModel>(maintenance);
            return Ok(viewModel);
        }

        /// <summary>
        /// Obtém todas as manutenções de um pedido específico.
        /// GET: api/maintenanceapi/order/{orderId}
        /// </summary>
        /// <param name="orderId">ID do pedido</param>
        /// <returns>Lista de manutenções do pedido</returns>
        [HttpGet("order/{orderId:guid}")]
        public async Task<IActionResult> GetByOrder(Guid orderId)
        {
            var maintenances = await _maintenanceRepository.GetMaintenancesByOrder(orderId);
            var viewModels = _mapper.Map<IEnumerable<MaintenanceApiViewModel>>(maintenances);
            return Ok(viewModels);
        }

        /// <summary>
        /// Obtém todo o histórico de manutenções de um cliente.
        /// GET: api/maintenanceapi/client/{clientId}
        /// </summary>
        /// <param name="clientId">ID do cliente</param>
        /// <returns>Lista de manutenções do cliente ordenadas por data</returns>
        [HttpGet("client/{clientId:guid}")]
        public async Task<IActionResult> GetByClient(Guid clientId)
        {
            var maintenances = await _maintenanceRepository.GetMaintenancesByClient(clientId);
            var viewModels = _mapper.Map<IEnumerable<MaintenanceApiViewModel>>(maintenances);
            return Ok(viewModels);
        }

        /// <summary>
        /// Cria uma nova manutenção.
        /// POST: api/maintenanceapi
        /// </summary>
        /// <param name="maintenanceViewModel">Dados da manutenção a criar</param>
        /// <returns>Mensagem de sucesso ou erros de validação</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] MaintenanceApiViewModel maintenanceViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var maintenance = _mapper.Map<Maintenance>(maintenanceViewModel);
                await _maintenanceService.Add(maintenance);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessCreationRegistration("Maintenance", maintenanceViewModel.Id.ToString())
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }

        /// <summary>
        /// Atualiza uma manutenção existente.
        /// PUT: api/maintenanceapi/{id}
        /// </summary>
        /// <param name="id">ID da manutenção a atualizar</param>
        /// <param name="maintenanceViewModel">Dados atualizados da manutenção</param>
        /// <returns>Mensagem de sucesso ou erros de validação</returns>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MaintenanceApiViewModel maintenanceViewModel)
        {
            if (id != maintenanceViewModel.Id)
                return BadRequest(new { success = false, message = "Maintenance Id invalid." });

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var maintenance = _mapper.Map<Maintenance>(maintenanceViewModel);
                await _maintenanceService.Update(maintenance);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessChangeRegistration("Maintenance", maintenanceViewModel.Id.ToString())
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }

        /// <summary>
        /// Remove uma manutenção.
        /// DELETE: api/maintenanceapi/{id}
        /// </summary>
        /// <param name="id">ID da manutenção a remover</param>
        /// <returns>Mensagem de sucesso ou erro se manutenção não encontrada</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var maintenance = await _maintenanceRepository.GetById(id);
                if (maintenance == null)
                    return NotFound(new { success = false, message = "Maintenance not found." });

                await _maintenanceService.Remove(id);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessRemovalRegistration("Maintenance", maintenance.Id.ToString())
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }
    }
}
