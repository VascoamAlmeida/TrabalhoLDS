using AutoMapper;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Parameters;
using LojaMaquinas.Business.Models.Parameters;
using LojaMaquinas.Helper.StaticVariables;
using LojaMaquinas.WebApp.ViewModels.MVC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LojaMaquinas.WebApp.Controllers.API
{
    [Authorize(Roles = "ADMIN")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class CategoryController : BaseController
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;

        public CategoryController(ICategoryRepository categoryRepository,
                                  ICategoryService categoryService,
                                  IMapper mapper,
                                  INotifierService notifier) : base(notifier)
        {
            _categoryRepository = categoryRepository;
            _categoryService = categoryService;
            _mapper = mapper;
        }

        [Route("category-list")]
        public async Task<IActionResult> Index()
        {
            var categories = await _categoryRepository.GetAll();

            return View(_mapper.Map<IEnumerable<CategoryViewModel>>(categories));
        }

        [Route("category-details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var categoryViewModel = await GetCategoryDetailsById(id);

            if (categoryViewModel == null) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

            return View(categoryViewModel);
        }

        #region Creation Methods
        [Route("category-create")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Route("category-create")]
        [HttpPost]
        public async Task<IActionResult> Create(CategoryViewModel categoryViewModel)
        {
            try
            {
                // 1. Validation of attributes by Model State
                if (ModelStateResponse() != null) return Json(new { success = false, message = "", errors = ModelStateResponse() });

                // 2. Map ViewModel entity to Model
                var category = _mapper.Map<Category>(categoryViewModel);

                // 3. Execute the creation order (persistence in the database) 
                await _categoryService.Add(category);

                // 4. Business rules validation response in the backend (fluent Validation + DataBase)
                if (PersistenceResponse() != null) return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = MessageHelper.Generics.SuccessCreationRegistration("Category", categoryViewModel.Name) });

            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion

        #region Edit Methods
        [Route("category-edit/{id:guid}")]
        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var categoryViewModel = await GetCategoryDetailsById(id);

            if (categoryViewModel != null) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

            return View(categoryViewModel);
        }

        [Route("category-edit/{id:guid}")]
        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, CategoryViewModel categoryViewModel)
        {
            try
            {
                // 1. Validation Id enter paramenter
                if (id != categoryViewModel.Id) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

                // 2. Validation of attributes by Model State
                if (ModelStateResponse() != null) return Json(new { success = false, message = "", errors = ModelStateResponse() });

                // 3. Map ViewModel entity to Model
                var category = _mapper.Map<Category>(categoryViewModel);

                // 4. Execute the update order (persistence in the database) 
                await _categoryRepository.Update(category);

                // 5. Business rules validation response in the backend (fluent Validation + DataBase)
                if (PersistenceResponse() != null) return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = MessageHelper.Generics.SuccessChangeRegistration("Perfil", categoryViewModel.Name) });
            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion

        #region Remove Methods
        [Route("category-remove/{id:guid}")]
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            try
            {
                // 1. Get entity by Id
                var categoryViewModel = await GetCategoryDetailsById(id);

                // 2. Validate if exist in database
                if (categoryViewModel == null) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

                // 3. Execute the remove (persistence in the database) 
                await _categoryService.Remove(id);

                // 4. Business rules validation response in the backend (fluent Validation + DataBase)
                if (PersistenceResponse() != null) return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = MessageHelper.Generics.SuccessRemovalRegistration("Category", categoryViewModel.Name) });
            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion

        #region Private Methods
        private async Task<CategoryViewModel> GetCategoryDetailsById(Guid id)
        {
            return _mapper.Map<CategoryViewModel>(await _categoryRepository.GetById(id));
        }
        #endregion
    }
}
