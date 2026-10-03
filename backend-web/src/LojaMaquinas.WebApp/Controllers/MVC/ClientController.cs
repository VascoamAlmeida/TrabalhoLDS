using AutoMapper;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Helper.StaticVariables;
using LojaMaquinas.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LojaMaquinas.WebApp.Controllers.MVC
{
    [Authorize(Roles = "ADMIN")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ClientController : BaseController
    {
        private readonly IClientRepository _clientRepository;
        private readonly IClientService _clientService;
        private readonly IMapper _mapper;

        public ClientController(IClientRepository clientRepository,
                                IClientService clientService,
                                IMapper mapper,
                                INotifierService notifier) : base(notifier)
        {
            _clientRepository = clientRepository;
            _clientService = clientService;
            _mapper = mapper;
        }

        [Route("client-list")]
        public async Task<IActionResult> Index()
        {
            var clients = await _clientRepository.GetAllClientsWithUsers();

            return View(_mapper.Map<IEnumerable<ClientViewModel>>(clients));
        }

        [Route("client-details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var clientViewModel = await GetClientDetailsById(id);

            if (clientViewModel == null) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

            return View(clientViewModel);
        }

        [Route("client-history/{id:guid}")]
        public async Task<IActionResult> History(Guid id)
        {
            var client = await _clientRepository.GetClientWithUserAndOrders(id);

            if (client == null) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

            return View(_mapper.Map<ClientViewModel>(client));
        }

        #region Creation Methods
        [Route("client-create")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Route("client-create")]
        [HttpPost]
        public async Task<IActionResult> Create(ClientViewModel clientViewModel)
        {
            try
            {
                // 1. Validation of attributes by Model State
                if (ModelStateResponse() != null) return Json(new { success = false, message = "", errors = ModelStateResponse() });

                // 2. Map ViewModel entity to Model
                var client = _mapper.Map<Client>(clientViewModel);

                // 3. Execute the creation order (persistence in the database)
                await _clientService.Add(client);

                // 4. Business rules validation response in the backend (fluent Validation + DataBase)
                if (PersistenceResponse() != null) return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = MessageHelper.Generics.SuccessCreationRegistration("Client", $"{clientViewModel.FirstName} {clientViewModel.LastName}") });

            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion

        #region Edit Methods
        [Route("client-edit/{id:guid}")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var clientViewModel = await GetClientDetailsById(id);

            if (clientViewModel == null) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

            return View(clientViewModel);
        }

        [Route("client-edit/{id:guid}")]
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, ClientViewModel clientViewModel)
        {
            try
            {
                // 1. Validation Id enter paramenter
                if (id != clientViewModel.Id) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

                // 2. Validation of attributes by Model State
                if (ModelStateResponse() != null) return Json(new { success = false, message = "", errors = ModelStateResponse() });

                // 3. Map ViewModel entity to Model
                var client = _mapper.Map<Client>(clientViewModel);

                // 4. Execute the update order (persistence in the database)
                await _clientService.Update(client);

                // 5. Business rules validation response in the backend (fluent Validation + DataBase)
                if (PersistenceResponse() != null) return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = MessageHelper.Generics.SuccessChangeRegistration("Client", $"{clientViewModel.FirstName} {clientViewModel.LastName}") });
            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion

        #region Remove Methods
        [Route("client-remove/{id:guid}")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                // 1. Get entity by Id
                var clientViewModel = await GetClientDetailsById(id);

                // 2. Validate if exist in database
                if (clientViewModel == null) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

                // 3. Execute the remove (persistence in the database)
                await _clientService.Remove(id);

                // 4. Business rules validation response in the backend (fluent Validation + DataBase)
                if (PersistenceResponse() != null) return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = MessageHelper.Generics.SuccessRemovalRegistration("Client", $"{clientViewModel.FirstName} {clientViewModel.LastName}") });
            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion

        #region Private Methods
        private async Task<ClientViewModel> GetClientDetailsById(Guid id)
        {
            return _mapper.Map<ClientViewModel>(await _clientRepository.GetById(id));
        }
        #endregion
    }
}
