using AutoMapper;
using LojaMaquinas.Business.Interfaces.Parameters;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Models.Parameters;
using LojaMaquinas.Helper.StaticVariables;
using LojaMaquinas.WebApp.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LojaMaquinas.WebApp.ViewModels.API;
using LojaMaquinas.Business.Models.Entities;

namespace LojaMaquinas.WebApp.Controllers.API
{
    /// <summary>
    /// API Controller para gestão de categorias de produtos.
    /// Fornece endpoints CRUD completos para categorias.
    /// </summary>
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CategoryApiController : BaseApiController
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICategoryService _categoryService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Inicializa o controller de categorias com dependências necessárias.
        /// </summary>
        /// <param name="categoryRepository">Repositório de acesso a dados de categorias</param>
        /// <param name="categoryService">Serviço de lógica de negócio de categorias</param>
        /// <param name="mapper">AutoMapper para conversão entre entidades e ViewModels</param>
        /// <param name="notifier">Serviço de notificações de erro</param>
        public CategoryApiController(ICategoryRepository categoryRepository,
                                  ICategoryService categoryService,
                                  IMapper mapper,
                                  INotifierService notifier) : base(notifier)
        {
            _categoryRepository = categoryRepository;
            _categoryService = categoryService;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtém todas as categorias.
        /// GET: api/v1/categoryapi
        /// </summary>
        /// <returns>Lista de todas as categorias</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _categoryRepository.GetAll();

            var viewModels = _mapper.Map<IEnumerable<CategoryApiViewModel>>(categories);
            return Ok(viewModels);
        }

        /// <summary>
        /// Obtém uma categoria específica por ID.
        /// GET: api/v1/categoryapi/{id}
        /// </summary>
        /// <param name="id">ID único da categoria</param>
        /// <returns>Categoria encontrada ou NotFound</returns>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var category = await _categoryRepository.GetById(id);

            if (category == null)
                return NotFound(new { success = false, message = "Category not found." });

            var viewModel = _mapper.Map<CategoryApiViewModel>(category);
            return Ok(viewModel);
        }

        /// <summary>
        /// Cria uma nova categoria.
        /// POST: api/v1/categoryapi
        /// </summary>
        /// <param name="categoryViewModel">Dados da categoria a criar</param>
        /// <returns>Mensagem de sucesso ou erros de validação</returns>
        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryApiViewModel categoryViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var category = _mapper.Map<Category>(categoryViewModel);
                await _categoryService.Add(category);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessCreationRegistration("Category", categoryViewModel.Id.ToString())
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }

        /// <summary>
        /// Atualiza uma categoria existente.
        /// PUT: api/v1/categoryapi/{id}
        /// </summary>
        /// <param name="id">ID da categoria a atualizar</param>
        /// <param name="categoryViewModel">Dados atualizados da categoria</param>
        /// <returns>Mensagem de sucesso ou erros de validação</returns>
        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CategoryApiViewModel categoryViewModel)
        {
            if (id != categoryViewModel.Id)
                return BadRequest(new { success = false, message = "Category Id invalid." });

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var category = _mapper.Map<Category>(categoryViewModel);
                await _categoryService.Update(category);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });
                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessChangeRegistration("Category", categoryViewModel.Id.ToString())
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }

        /// <summary>
        /// Remove uma categoria.
        /// DELETE: api/v1/categoryapi/{id}
        /// </summary>
        /// <param name="id">ID da categoria a remover</param>
        /// <returns>Mensagem de sucesso ou erro se categoria não encontrada</returns>
        [Authorize(Roles = "ADMIN")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var category = await _categoryRepository.GetById(id);
                if (category == null)
                    return NotFound(new { success = false, message = "Category not found." });

                await _categoryService.Remove(id);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessRemovalRegistration("Category", category.Id.ToString())
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
