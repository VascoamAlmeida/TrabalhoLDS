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
    /// API Controller para gestão de clientes.
    /// Fornece endpoints CRUD completos + consultas por NIF, UserId e alugueres a expirar.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ClientApiController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IClientRepository _clientRepository;
        private readonly IClientService _clientService;

        /// <summary>
        /// Inicializa o controller de clientes com dependências necessárias.
        /// </summary>
        /// <param name="mapper">AutoMapper para conversão entre entidades e ViewModels</param>
        /// <param name="notifier">Serviço de notificações de erro</param>
        /// <param name="clientRepository">Repositório de acesso a dados de clientes</param>
        /// <param name="clientService">Serviço de lógica de negócio de clientes</param>
        public ClientApiController(
            IMapper mapper,
            INotifierService notifier,
            IClientRepository clientRepository,
            IClientService clientService
        ) : base(notifier)
        {
            _mapper = mapper;
            _clientRepository = clientRepository;
            _clientService = clientService;
        }

        /// <summary>
        /// Verifica se o utilizador autenticado pode aceder aos dados do cliente.
        /// Administradores podem aceder a qualquer cliente; clientes apenas ao próprio perfil.
        /// </summary>
        private bool CanAccessClient(Client client)
        {
            return IsAdmin || (AuthenticatedUserId.HasValue && client.UserId == AuthenticatedUserId.Value);
        }

        /// <summary>
        /// Obtém todos os clientes com utilizadores associados.
        /// GET: api/clientapi
        /// </summary>
        /// <returns>Lista de todos os clientes com dados de User</returns>
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetAll()
        {
            var clients = await _clientRepository.GetAllClientsWithUsers();
            var viewModels = _mapper.Map<IEnumerable<ClientApiViewModel>>(clients);
            return Ok(viewModels);
        }

        /// <summary>
        /// Obtém um cliente específico por ID.
        /// GET: api/clientapi/{id}
        /// </summary>
        /// <param name="id">ID único do cliente</param>
        /// <returns>Cliente encontrado ou NotFound</returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var client = await _clientRepository.GetById(id);
            if (client == null)
                return NotFound(new { success = false, message = "Client not found." });

            if (!CanAccessClient(client))
                return Forbid();

            var viewModel = _mapper.Map<ClientApiViewModel>(client);
            return Ok(viewModel);
        }

        /// <summary>
        /// Obtém um cliente por NIF (Número de Identificação Fiscal).
        /// GET: api/clientapi/nif/{nif}
        /// </summary>
        /// <param name="nif">NIF do cliente (9 dígitos)</param>
        /// <returns>Cliente encontrado ou NotFound</returns>
        [HttpGet("nif/{nif}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> GetByNif(string nif)
        {
            var client = await _clientRepository.GetByNif(nif);
            if (client == null)
                return NotFound(new { success = false, message = "Client not found." });

            var viewModel = _mapper.Map<ClientApiViewModel>(client);
            return Ok(viewModel);
        }

        /// <summary>
        /// Obtém o cliente associado a um utilizador específico.
        /// GET: api/clientapi/user/{userId}
        /// </summary>
        /// <param name="userId">ID do utilizador</param>
        /// <returns>Cliente associado ao utilizador ou NotFound</returns>
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            if (!IsAdmin && (!AuthenticatedUserId.HasValue || AuthenticatedUserId.Value != userId))
                return Forbid();

            var client = await _clientRepository.GetByUserId(userId);
            if (client == null)
                return NotFound(new { success = false, message = "Client not found for this user." });

            var viewModel = _mapper.Map<ClientApiViewModel>(client);
            return Ok(viewModel);
        }

        /// <summary>
        /// Cria um novo cliente.
        /// POST: api/clientapi
        /// </summary>
        /// <param name="clientViewModel">Dados do cliente a criar</param>
        /// <returns>Mensagem de sucesso ou erros de validação</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ClientApiViewModel clientViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                if (!IsAdmin && (!AuthenticatedUserId.HasValue || clientViewModel.UserId != AuthenticatedUserId.Value))
                    return Forbid();

                var client = _mapper.Map<Client>(clientViewModel);
                await _clientService.Add(client);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessCreationRegistration("Client", $"{clientViewModel.FirstName} {clientViewModel.LastName}")
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, error = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Atualiza um cliente existente.
        /// PUT: api/clientapi/{id}
        /// </summary>
        /// <param name="id">ID do cliente a atualizar</param>
        /// <param name="clientViewModel">Dados atualizados do cliente</param>
        /// <returns>Mensagem de sucesso ou erros de validação</returns>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ClientApiViewModel clientViewModel)
        {
            if (id != clientViewModel.Id)
                return BadRequest(new { success = false, message = "Client Id invalid." });

            try
            {
                var existingClient = await _clientRepository.GetById(id);
                if (existingClient == null)
                    return NotFound(new { success = false, message = "Client not found." });

                if (!CanAccessClient(existingClient))
                    return Forbid();

                if (!IsAdmin && clientViewModel.UserId != existingClient.UserId)
                    return BadRequest(new { success = false, message = "The associated user cannot be changed." });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var client = _mapper.Map<Client>(clientViewModel);
                await _clientService.Update(client);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessChangeRegistration("Client", $"{clientViewModel.FirstName} {clientViewModel.LastName}")
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, error = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Remove um cliente.
        /// DELETE: api/clientapi/{id}
        /// </summary>
        /// <param name="id">ID do cliente a remover</param>
        /// <returns>Mensagem de sucesso ou erro se cliente não encontrado</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var client = await _clientRepository.GetById(id);
                if (client == null)
                    return NotFound(new { success = false, message = "Client not found." });

                if (!CanAccessClient(client))
                    return Forbid();

                await _clientService.Remove(id);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessRemovalRegistration("Client", $"{client.FirstName} {client.LastName}")
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new { success = false, error = "An unexpected error occurred." });
            }
        }

        /// <summary>
        /// Obtém alugueres (OrderRents) a expirar de um cliente específico.
        /// GET: api/clientapi/{id}/expiring-rents
        /// </summary>
        /// <param name="id">ID do cliente</param>
        /// <returns>Lista de alugueres que estão próximos da data de expiração</returns>
        [HttpGet("{id:guid}/expiring-rents")]
        public async Task<IActionResult> GetExpiringRentsByClientId(Guid id)
        {
            var client = await _clientRepository.GetById(id);
            if (client == null)
                return NotFound(new { success = false, message = "Client not found." });

            if (!CanAccessClient(client))
                return Forbid();

            var expiringRents = await _clientService.GetExpiringOrderRents(id);

            // Check for notifications
            var errors = PersistenceResponse();
            if (errors != null)
            {
                if (errors.Any(e => e.Contains("Client not found")))
                    return NotFound(new { success = false, errors });
            }

            var viewModels = _mapper.Map<IEnumerable<OrderRentApiViewModel>>(expiringRents);

            return Ok(viewModels);
        }
    }
}
