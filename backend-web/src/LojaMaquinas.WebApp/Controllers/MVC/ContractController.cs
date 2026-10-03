using AutoMapper;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Services.Entities;
using LojaMaquinas.Data.Repositories.Entities;
using LojaMaquinas.WebApp.ViewModels.MVC;
using LojaMaquinas.WebApp.ViewModels.MVC.Contracts;
using LojaMaquinas.WebApp.ViewModels.MVC.OrderRent;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LojaMaquinas.WebApp.Controllers.MVC
{
	public class ContractController : Controller
	{

		private readonly IContractRepository _contractRepository;
		private readonly IContractService _contractService;
		private readonly IOrderRentRepository _orderRentRepository;
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

		/// <summary>
        /// Inicializa o controller de contratos com dependências necessárias.
        /// </summary>
        /// <param name="contractRepository">Repositório de acesso a dados de contratos</param>
        /// <param name="contractService">Serviço de lógica de negócio de contratos</param>
        /// <param name="mapper">AutoMapper para conversão entre entidades e ViewModels</param>
        public ContractController(IContractRepository contractRepository, IContractService contractService, IMapper mapper,
             IOrderRentRepository orderRentRepository, IUserRepository userRepository)
        {
            _contractRepository = contractRepository;
            _contractService = contractService;
            _orderRentRepository = orderRentRepository;
            _mapper = mapper;
            _userRepository = userRepository;
        }
        /// <summary>
        /// Mostra formulário de criação de um pedido de orderRent
        /// </summary>
        /// <param name="machineId">Id da maquina a alugar</param>
        /// <returns>O formulario</returns>
        [Route("contract-create")]
		[HttpGet]
		public async Task<IActionResult> Get(Guid orderRentId)
		{
			var orderRent = await _orderRentRepository.GetOrderRentAndOrderAndContractByOrderRentId(orderRentId);

			var order = orderRent.Order;
			var client = order.Client;
			var product = order.Stock.Product;

			var durationDays = (int)(orderRent.EndDate - orderRent.InitialDate).TotalDays;

			var contract = new ContractViewModel
			{
				OrderRentId = orderRent.Id,
				MachineName = product.Name,
				MachineModel = product.Model,
				StartDate = orderRent.InitialDate,
				EndDate = orderRent.EndDate,
				Duration = durationDays,
				DailyPrice = order.Total / durationDays,
				Total = orderRent.Order.Total,
				TermsConditions = "Termos e Condições – Aluguer de Máquinas\r\nO aluguer de máquinas é fornecido pela empresa ao cliente para fins profissionais ou particulares. O período começa na entrega e termina na devolução acordada. O transporte é oferecido pela empresa.",
				ClientName = client.FirstName,
				ClientEmail = client.User.Email,
				ClientPhone = client.PhoneNumber,
				ClientAddress = client.Address
			};

			return View("Create", contract);
		}

		/// <summary>
		/// Recebe o formulario template e cria registo na BD
		/// </summary>
		/// <param name="createContractViewModel">Dados que sao preenchidos</param>
		/// <returns>Envia para pagina sucesso ou dá erro </returns>
		[Route("contract-create")]
		[HttpPost]
		public async Task<IActionResult> Create(ContractViewModel createContractViewModel)
		{
			// Validar modelo
			if (!ModelState.IsValid)
			{
				return View("Create", createContractViewModel);
			}

			if (!createContractViewModel.AcceptTerms)
			{
				ModelState.AddModelError(nameof(createContractViewModel.AcceptTerms),
					"Tem de aceitar os termos para continuar.");
				return View("Create", createContractViewModel);
			}

			var contract = _mapper.Map<Contract>(createContractViewModel);

			await _contractRepository.Add(contract);

			// gera uma referência simples 
			var referenceRandom = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

			return RedirectToAction("Success", new
			{
				equipmentName = createContractViewModel.MachineName,
				total = createContractViewModel.Total,
				reference = referenceRandom
			});
		}

		/// <summary>
		/// Controller de sucesso de criação de contrato
		/// </summary>
		/// <param name="equipmentName">Nome do equiapmeno alugado</param>
		/// <param name="total">Total a ser pago</param>
		/// <param name="reference">Referencia da maquina</param>
		/// <returns>View da confirmaçao do pedido</returns>
		[Route("contract-success")]
		[HttpGet]
		public IActionResult Success(string equipmentName, decimal total, string reference)
		{
			var model = new ContractSuccessViewModel
			{
				EquipmentName = equipmentName,
				Total = total,
				ReferenceNumber = reference
			};

			return View(model);
		}

		/// <summary>
		/// Page para apresentar a lista de todos os Contract
		/// </summary>
		/// <returns>Todas as Contract existentes</returns>
		[HttpGet]
		[Route("contract-list")]
        public async Task<IActionResult> Index()
        {
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            IEnumerable<Contract> contracts;

            if (userRole == "ADMIN")
            {
                contracts = await _contractRepository.GetAll();
            }
            else
            {
                var user = await _userRepository.GetById(userId);

                if (user == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                contracts = await _contractRepository.GetAllByClientEmail(user.Email);
            }

            var viewModels = _mapper.Map<IEnumerable<ContractViewModel>>(contracts);
            return View(viewModels);
        }

        /// <summary>
        /// Page para apresentar uma contract especifica
        /// </summary>
        /// <param name="id">Id do Contract que se pretende ver</param>
        /// <returns>Apresenta os detalhes de Contract especifica</returns>
        [HttpGet]
		[Route("contract-details/{id:guid}")]
		public async Task<IActionResult> Details(Guid id)
		{
			var contract = await _contractRepository.GetOrderRentIDbyContract(id);

			if (contract == null)
			{
				return NotFound();
			}

			// DEBUG / segurança: garantir que as navegações existem
			if (contract.OrderRent == null)
			{
				return Content("DEBUG: este contrato não tem OrderRent associado.");
			}

			var orderRent = contract.OrderRent;

			if (orderRent.Order == null)
			{
				return Content("DEBUG: este OrderRent não tem Order associado.");
			}

			var order = orderRent.Order;

			if (order.Client == null)
			{
				return Content("DEBUG: este Order não tem Client associado.");
			}

			var client = order.Client;

			if (client.User == null)
			{
				return Content("DEBUG: este Client não tem User associado.");
			}


			if (order.Stock == null)
			{
				return Content("DEBUG: este Order não tem Stock associado.");
			}

			var stock = order.Stock;

			if (stock.Product == null)
			{
				return Content("DEBUG: este Stock não tem Product associado.");
			}

			var product = stock.Product;


			var durationDays = (int)(orderRent.EndDate - orderRent.InitialDate).TotalDays;

			var result = new ContractViewModel
			{
				Id = contract.Id,
				OrderRentId = orderRent.Id,
				MachineName = product.Name,
				MachineModel = product.Model,
				StartDate = orderRent.InitialDate,
				EndDate = orderRent.EndDate,
				Duration = durationDays,
				DailyPrice = order.Total / durationDays,
				Total = order.Total,
				TermsConditions = contract.TermsConditions,
				ClientName = client.FirstName,
				ClientEmail = client.User.Email,
				ClientPhone = client.PhoneNumber,
				ClientAddress = client.Address
			};
			return View(result);

		}
	}
}
