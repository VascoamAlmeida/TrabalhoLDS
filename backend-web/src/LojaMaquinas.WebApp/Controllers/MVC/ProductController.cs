using AutoMapper;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Parameters;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Business.Models.Parameters;
using LojaMaquinas.Helper.StaticVariables;
using LojaMaquinas.WebApp.ViewModels.MVC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LojaMaquinas.WebApp.Controllers.MVC
{
    /// <summary>
    /// Controller para gestão de produtos.
    /// Fornece endpoints CRUD completos + filtros (tipo, categoria, disponibilidade) e pesquisa.
    /// </summary>
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ProductController : BaseController
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Inicializa o controller de produtos com dependências necessárias.
        /// </summary>
        /// <param name="productRepository">Repositório de acesso a dados de produtos</param>
        /// <param name="productService">Serviço de lógica de negócio de produtos</param>
        /// <param name="mapper">AutoMapper para conversão entre entidades e ViewModels</param>
        /// <param name="notifier">Serviço de notificações de erro</param>
        public ProductController(IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IProductService productService,
            IMapper mapper,
            INotifierService notifier) : base(notifier)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _productService = productService;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtém todos os produtos.
        /// GET: v1/products
        /// </summary>
        /// <returns>Lista de todos os produtos</returns>
        [AllowAnonymous]
        [Route("product-list")]
        public async Task<IActionResult> Index()
        {
            IEnumerable<Product> products = await _productRepository.GetAllProductsWithCategoryAndStocks();

            string userRole = null;
            Guid? userId = null;

            if (User.Identity.IsAuthenticated)
            {
                userRole = User.FindFirstValue(ClaimTypes.Role);
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!string.IsNullOrEmpty(userIdString))
                    userId = Guid.Parse(userIdString);
            }

            ViewBag.Role = userRole;

            return View(_mapper.Map<IEnumerable<ProductViewModel>>(products));
        }

        /// <summary>
        /// Obtém detalhes de um produto específico por ID.
        /// GET: v1/products/{id}
        /// </summary>
        /// <param name="id">ID único do produto</param>
        /// <returns>Produto encontrado ou NotFound</returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("product-details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            try
            {
                var productViewModel = await GetProductDetailsById(id);

                if (productViewModel == null)
                    return NotFound(new { success = false, message = "Product not found." });

                return PartialView("Details", productViewModel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        #region Creation Methods
        /// <summary>
        /// Direciona utilizador para página de criação de produto.
        /// GET: v1/products
        /// </summary>
        /// <returns>Página web</returns>
        [Route("product-create")]
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create()
        {
            IEnumerable<Category> categories;
            categories = await _categoryRepository.GetAll();
            ViewBag.Categories = categories;

            return View();
        }

        /// <summary>
        /// Cria um novo produto.
        /// POST: v1/products
        /// </summary>
        /// <param name="productViewModel">Dados do produto a criar</param>
        /// <returns>JSON a indicar sucesso ou erros de validação</returns>
        [Route("product-create")]
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(ProductViewModel productViewModel)
        {
            try
            {
                // 1. Validation of attributes by Model State
                if (ModelStateResponse() != null) return Json(new { success = false, message = "", errors = ModelStateResponse() });

                // 2. Map ViewModel entity to Model
                var product = _mapper.Map<Product>(productViewModel);

                // 3. Execute the creation order (persistence in the database)
                await _productService.Add(product);

                // 4. Business rules validation response in the backend (fluent Validation + DataBase)
                if (PersistenceResponse() != null) return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = MessageHelper.Generics.SuccessCreationRegistration("Product", $"{productViewModel.Name}") });

            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion

        #region Edit Methods
        /// <summary>
        /// Direciona utilizador para página de edição de produto.
        /// GET: v1/products
        /// </summary>
        /// <returns>Página web</returns>
        [Route("product-edit/{id:guid}")]
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var productViewModel = await GetProductDetailsById(id);
            IEnumerable<Category> categories;
            categories = await _categoryRepository.GetAll();
            ViewBag.Categories = categories;

            if (productViewModel == null) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

            return View(productViewModel);
        }

        /// <summary>
        /// Atualiza um produto existente.
        /// PUT: v1/products/{id}
        /// </summary>
        /// <param name="id">ID do produto a atualizar</param>
        /// <param name="productViewModel">Dados atualizados do produto</param>
        /// <returns>JSON a indicar sucesso ou erros de validação</returns>
        [Route("product-edit/{id:guid}")]
        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Edit(Guid id, ProductViewModel productViewModel)
        {
            try
            {
                // 1. Validation Id enter paramenter
                if (id != productViewModel.Id) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

                // 2. Validation of attributes by Model State
                if (ModelStateResponse() != null) return Json(new { success = false, message = "", errors = ModelStateResponse() });

                // 3. Map ViewModel entity to Model
                var product = _mapper.Map<Product>(productViewModel);

                // 4. Execute the update order (persistence in the database)
                await _productService.Update(product);

                // 5. Business rules validation response in the backend (fluent Validation + DataBase)
                if (PersistenceResponse() != null) return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = MessageHelper.Generics.SuccessChangeRegistration("Product", $"{productViewModel.Name}") });
            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion

        #region Remove Methods
        /// <summary>
        /// Remove um produto.
        /// DELETE: v1/products/{id}
        /// </summary>
        /// <param name="id">ID do produto a remover</param>
        /// <returns>Mensagem de sucesso ou erro se produto não encontrado</returns>
        [Route("product-delete/{id:guid}")]
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                // 1. Get entity by Id
                var productViewModel = await GetProductDetailsById(id);

                // 2. Validate if exist in database
                if (productViewModel == null) return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Id") });

                // 3. Execute the remove (persistence in the database)
                await _productService.Remove(id);

                // 4. Business rules validation response in the backend (fluent Validation + DataBase)
                if (PersistenceResponse() != null) return Json(new { success = false, message = "", errors = PersistenceResponse() });

                return Json(new { success = true, message = MessageHelper.Generics.SuccessRemovalRegistration("Product", $"{productViewModel.Name}") });
            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion

        #region Filtering/Search Methods
        /// <summary>
        /// Obtém produtos filtrados por tipo (Compra ou Aluguer).
        /// GET: v1/products/products-by-type/{type}
        /// </summary>
        /// <param name="type">Tipo de produto</param>
        /// <returns>Lista de produtos do tipo especificado</returns>
        [AllowAnonymous]
        [HttpGet("products-by-type/{type:int}")]
        public async Task<IActionResult> ListByType(int type)
        {
            try
            {
                if (!Enum.IsDefined(typeof(ProductType), type))
                {
                    return Json(new { success = false, error = MessageHelper.Generics.ErrorParameterNotFound("Product Type") });
                }

                var productType = (ProductType)type;
                var products = await _productRepository.GetProductsByType(productType);

                if (products == null || !products.Any())
                    return Json(new { success = false, message = "No products found for the specified type." });

                var viewModel = _mapper.Map<IEnumerable<ProductViewModel>>(products);
                return Json(new { success = true, data = viewModel, count = viewModel.Count() });
            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Pesquisa produtos por nome.
        /// GET: v1/products/products-search?searchTerm=...
        /// </summary>
        /// <param name="searchTerm">Termo de pesquisa</param>
        /// <returns>Lista de produtos que correspondem ao termo de pesquisa</returns>
        [AllowAnonymous]
        [HttpGet("products-search")]
        public async Task<IActionResult> Search(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return Json(new { success = false, error = "Search term is required." });
                }

                var products = await _productRepository.SearchProducts(searchTerm);

                if (products == null || !products.Any())
                    return Json(new { success = false, message = "No products found matching your search criteria." });

                var viewModel = _mapper.Map<IEnumerable<ProductViewModel>>(products);
                return Json(new { success = true, data = viewModel, count = viewModel.Count(), searchTerm = searchTerm });
            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Obtém produtos filtrados por categoria.
        /// GET: v1/products/category/{categoryId}
        /// </summary>
        /// <param name="categoryId">ID da categoria</param>
        /// <returns>Lista de produtos da categoria especificada</returns>
        [AllowAnonymous]
        [HttpGet("category/{categoryId:guid}")]
        public async Task<IActionResult> ListByCategory(Guid categoryId)
        {
            try
            {
                var products = await _productRepository.GetProductsByCategory(categoryId);

                if (products == null || !products.Any())
                    return Json(new { success = false, message = "No products found for the specified category." });

                var viewModel = _mapper.Map<IEnumerable<ProductViewModel>>(products);
                return Json(new { success = true, data = viewModel, count = viewModel.Count(), categoryId = categoryId });
            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }

        /// <summary>
        /// Obtém apenas produtos disponíveis.
        /// GET: v1/products/available
        /// </summary>
        /// <returns>Lista de produtos com stock disponível</returns>
        [AllowAnonymous]
        [HttpGet("available")]
        public async Task<IActionResult> ListAvailable()
        {
            try
            {
                var products = await _productRepository.GetAvailableProducts();

                if (products == null || !products.Any())
                    return Json(new { success = false, message = "No available products found." });

                var viewModel = _mapper.Map<IEnumerable<ProductViewModel>>(products);
                return Json(new { success = true, data = viewModel, count = viewModel.Count() });
            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }
        #endregion

        /// <summary>
        /// Obtém produtos com stock baixo.
        /// GET: v1/products/low-stock-products
        /// </summary>
        /// <returns>Lista de produtos com stock abaixo do limite definido</returns>
        [Authorize(Roles = "ADMIN")]
        [HttpGet("low-stock-products")]
        public async Task<IActionResult> LowStockAlert()
        {
            try
            {
                var lowStockProducts = await _productService.GetLowStockProducts();

                if (lowStockProducts == null || !lowStockProducts.Any())
                    return Json(new { success = false, message = "No low stock products found." });

                var productViewModels = _mapper.Map<IEnumerable<ProductViewModel>>(lowStockProducts);
                return Json(new { success = true, data = productViewModels, count = productViewModels.Count() });
            }
            catch (Exception ex)
            {
                //AuditLog(LogType.Error, ex.Message);
                return Json(new { success = false, error = ex.Message });
            }
        }

        #region Private Methods
        /// <summary>
        /// Método auxiliar privado para obter detalhes de um produto por ID.
        /// </summary>
        /// <param name="id">ID do produto</param>
        /// <returns>ViewModel do produto ou null se não encontrado</returns>
        private async Task<ProductViewModel> GetProductDetailsById(Guid id)
        {
            return _mapper.Map<ProductViewModel>(await _productRepository.GetById(id));
        }
        #endregion
    }
}