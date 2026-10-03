using FluentAssertions;
using LojaMaquinas.WebApp.ViewModels.API;
using System.Net.Http.Json;

namespace IntegrationTests.ApiTests
{

    /// <summary>
    /// Testes de integração de category
    /// </summary>
    public class CategoryApiTests : IClassFixture<IntegrationTestWebAppFactory>
    {
        private readonly IntegrationTestWebAppFactory _factory;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Construtor de factory
        /// </summary>
        /// <param name="factory">factory a ser utilizada</param>
        public CategoryApiTests(IntegrationTestWebAppFactory factory)
        {
            _factory = factory;
            _httpClient = _factory.CreateAuthenticatedClient();
            _factory.ResetDatabase();
        }

        /// <summary>
        /// Apresenta todas category
        /// </summary>
        /// <returns>Devolve OK, todas as category</returns>
        [Fact]
        public async Task GetAllCategories_ReturnOK()
        {
            // Arrange
            // Nao preciso de nada, já tenho da minha seeding

            // Act
            // Faz GET do endpoint
            var response = await _httpClient.GetAsync("/api/v1/CategoryApi");

            // Assert
            //Verifica se a resposta é OK
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        }

        /// <summary>
        /// Apresenta category por Id
        /// </summary>
        /// <returns>Devolve OK, pois a category pedida</returns>
        [Fact]
        public async Task GetCategoryById_WhenExists_ReturnOK()
        {
            // Arrange
            // Criar um Guid para uma Categoria
            var allCategories = await _httpClient.GetFromJsonAsync<IEnumerable<CategoryApiViewModel>>("/api/v1/CategoryApi");
            var categoryId = allCategories!.First().Id;

            // Act
            // Faz GET do endpoint
            var response = await _httpClient.GetAsync($"/api/v1/CategoryApi/{categoryId}");

            // Assert
            //Verifica se a resposta é OK
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        /// Não apresenta category por Id, pois id nao existe
        /// </summary>
        /// <returns>Devolve NotFound, pois a category Id pedida não existe</returns>
        [Fact]
        public async Task GetCategoryById_WhenDoesNotExist_ReturnNotFound()
        {
            // Arrange
            // Criar um Guid para uma nova order
            var categoryId = Guid.Parse("aaaaaaaa-aaaa-aaaa-bccb-aaaaaaaaaaaa");

            // Act
            // Faz GET do endpoint
            var response = await _httpClient.GetAsync($"/api/v1/CategoryApi/{categoryId}");

            // Assert
            //Verifica se a resposta é Not Found
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Cria a category pedida
        /// </summary>
        /// <returns>Devolve OK, pois category valida</returns>
        [Fact]
        public async Task CreateCategory_WhenValid_ReturnOK()
        {
            // Arrange
            // Faz uma nova category para criar
            var newCategory = new CategoryApiViewModel()
            {
                Id = Guid.NewGuid(),
                Name = "NewCategory",
                Description = "New category for tests"
            };

            // Act
            // Faz POST do endpoint
            var response = await _httpClient.PostAsJsonAsync("/api/v1/CategoryApi", newCategory);

            // Assert
            //Verifica se a resposta é OK
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        /// Não cria nova category pois tem dados invalidos
        /// </summary>
        /// <returns>Devolve BadRequest, pois nome é invalido</returns>
        [Fact]
        public async Task CreateCategory_ReturnBadRequest_NameInvalid()
        {
            // Arrange
            // Faz uma nova category para criar
            var newCategory = new CategoryApiViewModel()
            {
                Id = Guid.NewGuid(),
                Name = "",
                Description = "New category for tests"
            };

            // Act
            // Faz POST do endpoint
            var response = await _httpClient.PostAsJsonAsync("/api/v1/CategoryApi", newCategory);

            // Assert
            //Verifica se a resposta é Internal Server Error
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Permite atualizar category
        /// </summary>
        /// <returns>Devolve OK, pois category porque nome é diferente</returns>
        [Fact]
        public async Task UpdateCategory_WhenValid_ReturnOK()
        {
            // Arrange
            // Vai buscar todas as categorys da bd InMemory
            var category = await _httpClient.GetFromJsonAsync<IEnumerable<CategoryApiViewModel>>("/api/v1/CategoryApi");
            var newCategory = category!.First();

            // Que estou a mudar
            newCategory.Name = "diferente";

            // Act
            // Faz PUT do endpoint
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/CategoryApi/{newCategory.Id}", newCategory);

            // Assert
            //Verifica se a resposta é OK
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        ///  Não permite atualizar category
        /// </summary>
        /// <returns>ReturnBadRequest, id invalido/returns>
        [Fact]
        public async Task UpdateCategory_WhenIdMismatch_ReturnBadRequest()
        {
            // Arrange
            var categories = await _httpClient.GetFromJsonAsync<IEnumerable<CategoryApiViewModel>>("/api/v1/CategoryApi");
            var category = categories!.First();

            var differentId = Guid.NewGuid();

            // Act
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/CategoryApi/{differentId}", category);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Não permite remover category
        /// </summary>
        /// <returns>Devolve NotFound, pois id nao existe</returns>
        [Fact]
        public async Task DeleteCategory_WhenIdDoesNotExist_ReturnNotFound()
        {
            // Arrange
            // Criar um Guid para category
            var categoryId = Guid.Parse("aaaaaaaa-adda-abca-aaaa-aaaaaaaaaaaa");

            // Act
            // Faz DELETE do endpoint
            var response = await _httpClient.DeleteAsync($"/api/v1/CategoryApi/{categoryId}");

            // Assert
            //Verifica se a resposta é Not Found
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

        }
        /// <summary>
        /// Permite remover category
        /// </summary>
        /// <returns>Return OK, pois não tem produtos com essa category</returns>
        [Fact]
        public async Task DeleteCategory_ReturnOK_CategoryDoesNotHaveProducts()
        {
            // Arrange
            var newCategory = new CategoryApiViewModel
            {
                Id = Guid.NewGuid(),
                Name = "TempToDelete",
                Description = "Category temporária para teste de delete"
            };

            await _httpClient.PostAsJsonAsync("/api/v1/CategoryApi", newCategory);
            // Act
            // Faz DELETE do endpoint
            var response = await _httpClient.DeleteAsync($"/api/v1/CategoryApi/{newCategory.Id}");

            // Assert
            //Verifica se a resposta é Bad Request
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        }

        /// <summary>
        /// Não permite remover category
        /// </summary>
        /// <returns>Return BadRequest, pois category tem produtos </returns>
        [Fact]
        public async Task DeleteCategory_ReturnBadRequest_CategoryHasProducts()
        {
            // Arrange
            // Vai buscar todas as category da bd InMemory
            var category = await _httpClient.GetFromJsonAsync<IEnumerable<CategoryApiViewModel>>("/api/v1/CategoryApi");
            // Escolhe o Id de uma category
            var ccategoryId = category!.First().Id;

            // Act
            // Faz DELETE do endpoint
            var response = await _httpClient.DeleteAsync($"/api/v1/CategoryApi/{ccategoryId}");

            // Assert
            //Verifica se a resposta é Bad Request
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        }

    }
}
