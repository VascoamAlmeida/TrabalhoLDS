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
    /// API Controller para gestão de alugueres (OrderRent).
    /// Fornece endpoints CRUD completos para pedidos de aluguer.
    /// </summary>
    [Route("api/v1/orderrent")]
    [Authorize(Roles = "ADMIN")]
    [ApiController]
    public class OrderRentApiController : BaseApiController
    {

        private readonly IOrderRentRepository _orderRentRepository;
        private readonly IMapper _mapper;

        /// <summary>
        /// Inicializa o controller de alugueres com dependências necessárias.
        /// </summary>
        /// <param name="mapper">AutoMapper para conversão entre entidades e ViewModels</param>
        /// <param name="orderRentRepository">Repositório de acesso a dados de alugueres</param>
        /// <param name="notifier">Serviço de notificações de erro</param>
        public OrderRentApiController(IMapper mapper, IOrderRentRepository orderRentRepository, INotifierService notifier) : base (notifier)
        {
            _mapper = mapper;
            _orderRentRepository = orderRentRepository;
        }

        /// <summary>
        /// Obtém todos os alugueres.
        /// GET: api/v1/orderrent
        /// </summary>
        /// <returns>Lista de todos os alugueres</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var orderRent = await _orderRentRepository.GetAll();
            var viewModels = _mapper.Map<IEnumerable<OrderRentApiViewModel>>(orderRent);
            return Ok(viewModels);
        }

        /// <summary>
        /// Obtém um aluguer específico por ID.
        /// GET: api/v1/orderrent/{id}
        /// </summary>
        /// <param name="id">ID único do aluguer</param>
        /// <returns>Aluguer encontrado ou NotFound</returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var orderRent = await _orderRentRepository.GetById(id);
            if (orderRent == null)
                return NotFound(new { success = false, message = "Order rent not found." });

            var viewModel = _mapper.Map<OrderRentApiViewModel>(orderRent);
            return Ok(viewModel);
        }

        /// <summary>
        /// Cria um novo aluguer.
        /// POST: api/v1/orderrent
        /// </summary>
        /// <param name="orderRentViewModel">Dados do aluguer a criar</param>
        /// <returns>Mensagem de sucesso ou erros de validação</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OrderRentApiViewModel orderRentViewModel)
        {
            try
            {
                // Verifica dados no ViewModel
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var orderRent = _mapper.Map<OrderRent>(orderRentViewModel);
                await _orderRentRepository.Add(orderRent); // Vai buscar as regras de negócio

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                //return Ok();
                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessCreationRegistration("OrderRent", orderRentViewModel.Id.ToString())
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }

        /// <summary>
        /// Atualiza um aluguer existente.
        /// PUT: api/v1/orderrent/{id}
        /// </summary>
        /// <param name="id">ID do aluguer a atualizar</param>
        /// <param name="OrderRentViewModel">Dados atualizados do aluguer</param>
        /// <returns>Mensagem de sucesso ou erros de validação</returns>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] OrderRentApiViewModel OrderRentViewModel)
        {
            if (id != OrderRentViewModel.Id)
                return BadRequest(new { success = false, message = "Id da Order Rent invalid." });

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var orderRent = _mapper.Map<OrderRent>(OrderRentViewModel);
                await _orderRentRepository.Update(orderRent);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });


                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessChangeRegistration("OrderRent", OrderRentViewModel.Id.ToString())
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }

        /// <summary>
        /// Remove um aluguer.
        /// DELETE: api/v1/orderrent/{id}
        /// </summary>
        /// <param name="id">ID do aluguer a remover</param>
        /// <returns>Mensagem de sucesso ou erro se aluguer não encontrado</returns>
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var orderRent = await _orderRentRepository.GetById(id);
                if (orderRent == null)
                    return NotFound(new { success = false, message = "orderRent not found." });

                await _orderRentRepository.Remove(id);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                //return Ok();
                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessRemovalRegistration("OrderRent", orderRent.Id.ToString())
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
