using AutoMapper;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Helper.StaticVariables;
using LojaMaquinas.WebApp.ViewModels.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LojaMaquinas.WebApp.Controllers.API
{
    /// <summary>
    /// API Controller para gestão de contratos de aluguer.
    /// Fornece endpoints CRUD completos. Contratos são gerados automaticamente a partir de OrderRentId.
    /// </summary>
    [Route("api/v1/contract")]
    [Authorize(Roles = "ADMIN")]
    [ApiController]
    public class ContractApiController : BaseApiController
    {
        private readonly IContractRepository _contractRepository;
        private readonly IContractService _contractService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Inicializa o controller de contratos com dependências necessárias.
        /// </summary>
        /// <param name="contractRepository">Repositório de acesso a dados de contratos</param>
        /// <param name="contractService">Serviço de lógica de negócio de contratos</param>
        /// <param name="mapper">AutoMapper para conversão entre entidades e ViewModels</param>
        /// <param name="notifier">Serviço de notificações de erro</param>
        public ContractApiController(IContractRepository contractRepository, IContractService contractService, IMapper mapper, INotifierService notifier) : base(notifier)
        {
            _contractRepository = contractRepository;
            _contractService = contractService;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtém todos os contratos.
        /// GET: api/v1/contract
        /// </summary>
        /// <returns>Lista de todos os contratos</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var contract = await _contractRepository.GetAll();
            var viewModels = _mapper.Map<IEnumerable<ContractApiViewModel>>(contract);
            return Ok(viewModels);
        }

        /// <summary>
        /// Obtém um contrato específico por ID.
        /// GET: api/v1/contract/{id}
        /// </summary>
        /// <param name="id">ID único do contrato</param>
        /// <returns>Contrato encontrado ou NotFound</returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var contract = await _contractRepository.GetById(id);
            if (contract == null)
                return NotFound(new { success = false, message = "Contract not found." });

            var viewModel = _mapper.Map<ContractApiViewModel>(contract);
            return Ok(viewModel);
        }

        /// <summary>
        /// Cria um novo contrato a partir de um OrderRentId.
        /// O contrato é gerado automaticamente com base nos dados do aluguer.
        /// POST: api/v1/contract
        /// </summary>
        /// <param name="contractViewModel">ViewModel contendo OrderRentId para gerar o contrato</param>
        /// <returns>Mensagem de sucesso ou erros de validação</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ContractApiViewModel contractViewModel)
        {
            try
            {
                // Verifica dados no ViewModel
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Cria contrato a partir do OrderRentId
                var contract = await _contractService.Add(contractViewModel.OrderRentId); // Vai buscar as regras de negócio


                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                //return Ok();
                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessCreationRegistration("Contract", contractViewModel.Id.ToString())
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }

        /// <summary>
        /// Atualiza os termos e condições de um contrato existente.
        /// PUT: api/v1/contract/{id}
        /// </summary>
        /// <param name="id">ID do contrato a atualizar</param>
        /// <param name="contractViewModel">Dados atualizados do contrato (TermsConditions)</param>
        /// <returns>Mensagem de sucesso ou erros de validação</returns>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ContractApiViewModel contractViewModel)
        {
            if (id != contractViewModel.Id)
                return BadRequest(new { success = false, message = "Id do Contract invalid." });

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                await _contractService.Update(contractViewModel.Id, contractViewModel.TermsConditions);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                //return Ok();
                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessChangeRegistration("Contract", contractViewModel.Id.ToString())
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                //await _audit.LogAsync(LogType.Error, errorMsg, profileViewModel);
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }

        /// <summary>
        /// Remove um contrato.
        /// DELETE: api/v1/contract/{id}
        /// </summary>
        /// <param name="id">ID do contrato a remover</param>
        /// <returns>Mensagem de sucesso ou erro se contrato não encontrado</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var contract = await _contractRepository.GetById(id);
                if (contract == null)
                    return NotFound(new { success = false, message = "contract not found." });

                await _contractService.Remove(id);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessRemovalRegistration("Contract", contract.Id.ToString())
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



