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
    public class UserController : BaseController
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserRepository userRepository,
                              IUserService userService,
                              IMapper mapper,
                              INotifierService notifier) : base(notifier)
        {
            _userRepository = userRepository;
            _userService = userService;
            _mapper = mapper;
        }

        [Route("user-list")]
        public async Task<IActionResult> Index()
        {
            var users = await _userRepository.GetAll();

            return View(_mapper.Map<IEnumerable<UserViewModel>>(users));
        }

        [Route("user-details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var userViewModel = await GetUserDetailsById(id);

            if (userViewModel == null) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

            return View(userViewModel);
        }

        #region Creation Methods
        [Route("user-create")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Route("user-create")]
        [HttpPost]
        public async Task<IActionResult> Create(UserViewModel userViewModel)
        {
            try
            {
                // 1. Validation of attributes by Model State
                if (ModelStateResponse() != null) return Json(new { success = false, message = "", errors = ModelStateResponse() });

                // 2. Map ViewModel entity to Model
                var user = _mapper.Map<User>(userViewModel);

                // 3. Execute the creation order (persistence in the database)
                await _userService.Add(user);

                // 4. Business rules validation response in the backend (fluent Validation + DataBase)
                if (PersistenceResponse() != null) return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = MessageHelper.Generics.SuccessCreationRegistration("User", userViewModel.Email) });

            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion

        #region Edit Methods
        [Route("user-edit/{id:guid}")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var userViewModel = await GetUserDetailsById(id);

            if (userViewModel == null) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

            return View(userViewModel);
        }

        [Route("user-edit/{id:guid}")]
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, UserViewModel userViewModel)
        {
            try
            {
                // 1. Validation Id enter paramenter
                if (id != userViewModel.Id) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

                // 2. Validation of attributes by Model State
                if (ModelStateResponse() != null) return Json(new { success = false, message = "", errors = ModelStateResponse() });

                // 3. Map ViewModel entity to Model
                var user = _mapper.Map<User>(userViewModel);

                // 4. Execute the update order (persistence in the database)
                await _userService.Update(user);

                // 5. Business rules validation response in the backend (fluent Validation + DataBase)
                if (PersistenceResponse() != null) return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = MessageHelper.Generics.SuccessChangeRegistration("User", userViewModel.Email) });
            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion

        #region Deactivate Methods
        [Route("user-deactivate/{id:guid}")]
        [HttpPost]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            try
            {
                // 1. Get entity by Id
                var user = await _userRepository.GetById(id);

                // 2. Validate if exist in database
                if (user == null) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

                // 3. Deactivate user
                user.IsActive = false;
                await _userService.Update(user);

                // 4. Business rules validation response in the backend (fluent Validation + DataBase)
                if (PersistenceResponse() != null) return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = $"User {user.Email} deactivated successfully!" });
            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion

        #region Remove Methods
        [Route("user-remove/{id:guid}")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                // 1. Get entity by Id
                var userViewModel = await GetUserDetailsById(id);

                // 2. Validate if exist in database
                if (userViewModel == null) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

                // 3. Execute the remove (persistence in the database)
                await _userService.Remove(id);

                // 4. Business rules validation response in the backend (fluent Validation + DataBase)
                if (PersistenceResponse() != null) return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = MessageHelper.Generics.SuccessRemovalRegistration("User", userViewModel.Email) });
            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion

        #region Private Methods
        private async Task<UserViewModel> GetUserDetailsById(Guid id)
        {
            return _mapper.Map<UserViewModel>(await _userRepository.GetById(id));
        }
        #endregion
    }
}
