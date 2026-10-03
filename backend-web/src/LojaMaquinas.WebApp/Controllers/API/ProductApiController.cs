using AutoMapper;
using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Parameters;
using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Helper.StaticVariables;
using LojaMaquinas.WebApp.ViewModels.MVC;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LojaMaquinas.WebApp.Controllers.API
{
    /// <summary>
    /// API Controller para gestão de produtos.
    /// Fornece endpoints CRUD completos + filtros (tipo, categoria, disponibilidade) e pesquisa.
    /// </summary>
    [Route("api/v1/products")]
    [ApiController]
    public class ProductApiController : BaseApiController
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        /// <summary>
        /// Inicializa o controller de produtos com dependências necessárias.
        /// </summary>
        /// <param name="productRepository">Repositório de acesso a dados de produtos</param>
        /// <param name="productService">Serviço de lógica de negócio de produtos</param>
        /// <param name="mapper">AutoMapper para conversão entre entidades e ViewModels</param>
        /// <param name="notifier">Serviço de notificações de erro</param>
        public ProductApiController(IProductRepository productRepository,
                                  IProductService productService,
                                  IMapper mapper,
                                  INotifierService notifier) : base(notifier)
        {
            _productRepository = productRepository;
            _productService = productService;
            _mapper = mapper;
        }

        /// <summary>
        /// Obtém todos os produtos.
        /// GET: api/v1/products
        /// </summary>
        /// <returns>Lista de todos os produtos</returns>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAll();

            var viewModels = _mapper.Map<IEnumerable<ProductApiViewModel>>(products);
            return Ok(viewModels);
        }

        /// <summary>
        /// Obtém todos os produtos com todos os detalhes(utilizado para catálogo mobile).
        /// GET: api/v1/products/mobile
        /// </summary>
        /// <returns>Lista de todos os produtos</returns>
        [AllowAnonymous]
        [HttpGet("mobile")]
        public async Task<IActionResult> IndexMobile()
        {
            var products = await _productRepository.GetAllProductsWithCategoryAndStocks();

            var viewModels = _mapper.Map<IEnumerable<ProductApiViewModelWithStocks>>(products);
            return Ok(viewModels);
        }

        /// <summary>
        /// Obtém detalhes de um produto específico por ID.
        /// GET: api/v1/products/{id}
        /// </summary>
        /// <param name="id">ID único do produto</param>
        /// <returns>Produto encontrado ou NotFound</returns>
        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var productViewModel = await GetProductDetailsById(id);

            if (productViewModel == null)
                return NotFound(new { success = false, message = "Product not found." });

            var viewModel = _mapper.Map<ProductApiViewModel>(productViewModel);
            return Ok(viewModel);
        }

        #region Creation Methods
        /// <summary>
        /// Cria um novo produto.
        /// POST: api/v1/products
        /// </summary>
        /// <param name="productViewModel">Dados do produto a criar</param>
        /// <returns>Mensagem de sucesso ou erros de validação</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductApiViewModel productViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var product = _mapper.Map<Product>(productViewModel);
                await _productService.Add(product);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessCreationRegistration("Product", productViewModel.Name)
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }
        #endregion

        #region Edit Methods
        /// <summary>
        /// Atualiza um produto existente.
        /// PUT: api/v1/products/{id}
        /// </summary>
        /// <param name="id">ID do produto a atualizar</param>
        /// <param name="productViewModel">Dados atualizados do produto</param>
        /// <returns>Mensagem de sucesso ou erros de validação</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] ProductApiViewModel productViewModel)
        {
            if (id != productViewModel.Id)
                return BadRequest(new { success = false, message = "Product id is invalid." });

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var product = _mapper.Map<Product>(productViewModel);
                await _productService.Update(product);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessChangeRegistration("Product", productViewModel.Name)
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }
        #endregion

        #region Remove Methods
        /// <summary>
        /// Remove um produto.
        /// DELETE: api/v1/products/{id}
        /// </summary>
        /// <param name="id">ID do produto a remover</param>
        /// <returns>Mensagem de sucesso ou erro se produto não encontrado</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var product = await _productRepository.GetById(id);
                if (product == null)
                    return NotFound(new { success = false, message = "Product not found." });

                await _productService.Remove(id);

                var errors = PersistenceResponse();
                if (errors != null)
                    return BadRequest(new { success = false, errors });

                return Ok(new
                {
                    success = true,
                    message = MessageHelper.Generics.SuccessRemovalRegistration("Product", product.Name)
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.Message.Length > 500 ? ex.Message[..497] + "..." : ex.Message;
                return StatusCode(500, new { success = false, error = errorMsg });
            }
        }
        #endregion

        #region Filtering/Search Methods
        /// <summary>
        /// Obtém produtos filtrados por tipo (Compra ou Aluguer).
        /// GET: api/v1/products/products-by-type/{type}
        /// </summary>
        /// <param name="type">Tipo de produto (enum ProductType: 1=Compra, 2=Aluguer)</param>
        /// <returns>Lista de produtos do tipo especificado</returns>
        [AllowAnonymous]
        [HttpGet("products-by-type/{type:int}")]
        public async Task<IActionResult> ListByType(int type)
        {
            if (!Enum.IsDefined(typeof(ProductType), type))
            {
                return NotFound();
            }
            var productType = (ProductType)type;

            var products = await _productRepository.GetProductsByType(productType);

            if (products == null)
                return NotFound(new { success = false, message = "Products not found." });

            var viewModel = _mapper.Map<IEnumerable<ProductApiViewModel>>(products);
            return Ok(viewModel);
        }

        /// <summary>
        /// Pesquisa produtos por termo (nome, marca ou modelo).
        /// GET: api/v1/products/products-search?searchTerm=...
        /// </summary>
        /// <param name="searchTerm">Termo de pesquisa</param>
        /// <returns>Lista de produtos que correspondem ao termo de pesquisa</returns>
        [AllowAnonymous]
        [HttpGet("products-search")]
        public async Task<IActionResult> Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RedirectToAction(nameof(Index));
            }

            var products = await _productRepository.SearchProducts(searchTerm);

            if (products == null)
                return NotFound(new { success = false, message = "Products not found." });

            var viewModel = _mapper.Map<IEnumerable<ProductApiViewModel>>(products);
            return Ok(viewModel);
        }

        /// <summary>
        /// Obtém produtos filtrados por categoria.
        /// GET: api/v1/products/category/{categoryId}
        /// </summary>
        /// <param name="categoryId">ID da categoria</param>
        /// <returns>Lista de produtos da categoria especificada</returns>
        [AllowAnonymous]
        [HttpGet("category/{categoryId:guid}")]
        public async Task<IActionResult> ListByCategory(Guid categoryId)
        {
            var products = await _productRepository.GetProductsByCategory(categoryId);

            if (products == null)
                return NotFound(new { success = false, message = "Products not found." });

            var viewModel = _mapper.Map<IEnumerable<ProductApiViewModel>>(products);
            return Ok(viewModel);
        }

        /// <summary>
        /// Obtém apenas produtos disponíveis (com stock).
        /// GET: api/v1/products/available
        /// </summary>
        /// <returns>Lista de produtos com stock disponível</returns>
        [AllowAnonymous]
        [HttpGet("available")]
        public async Task<IActionResult> ListAvailable()
        {
            var products = await _productRepository.GetAvailableProducts();

            if (products == null)
                return NotFound(new { success = false, message = "Products not found." });

            var viewModel = _mapper.Map<IEnumerable<ProductApiViewModel>>(products);
            return Ok(viewModel);
        }
        #endregion

        /// <summary>
        /// Obtém produtos com stock baixo (alerta de reposição).
        /// GET: api/v1/products/low-stock-products
        /// </summary>
        /// <returns>Lista de produtos com stock abaixo do limite definido</returns>
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "ADMIN")]
        [HttpGet("low-stock-products")]
        public async Task<IActionResult> LowStockAlert()
        {
            var lowStockProducts = await _productService.GetLowStockProducts();

            if (lowStockProducts == null)
                return NotFound(new { success = false, message = "Products not found." });

            var productViewModels = _mapper.Map<IEnumerable<ProductApiViewModel>>(lowStockProducts);
            return Ok(productViewModels);
        }

        #region Private Methods
        /// <summary>
        /// Método auxiliar privado para obter detalhes de um produto por ID.
        /// </summary>
        /// <param name="id">ID do produto</param>
        /// <returns>ViewModel do produto ou null se não encontrado</returns>
        private async Task<ProductApiViewModel> GetProductDetailsById(Guid id)
        {
            return _mapper.Map<ProductApiViewModel>(await _productRepository.GetById(id));
        }
        #endregion
    }
}
