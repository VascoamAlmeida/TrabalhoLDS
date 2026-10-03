using FluentAssertions;
using LojaMaquinas.WebApp.ViewModels.API;
using System.Net.Http.Json;

namespace IntegrationTests.ApiTests
{
    /// <summary>
    /// Testes de integração Contract 
    /// </summary>
    public class ContractApiTests : IClassFixture<IntegrationTestWebAppFactory>
    {
        private readonly IntegrationTestWebAppFactory _factory;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Construtor Factory.
        /// </summary>
        /// <param name="factory">Factory usado para testes.</param>
        public ContractApiTests(IntegrationTestWebAppFactory factory)
        {
            _factory = factory;
            _httpClient = _factory.CreateAuthenticatedClient();
            _factory.ResetDatabase();
        }

        /// <summary>
        /// Deve devolver a lista de contratos com sucesso.
        /// </summary>
        /// <returns>Devovle OK, todos os contratos</returns>
        [Fact]
        public async Task GetAllContracts_ReturnOK()
        {
            // Arrange
            // Nao preciso de nada, já tenho da minha seeding

            // Act
            // Faz GET do endpoint
            var response = await _httpClient.GetAsync("/api/v1/contract");


            // Assert
            //Verifica se a resposta é OK
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        /// Devolve um contract existente pelo Id.
        /// </summary>
        /// <returns>Devolve Ok, um contrato Id</returns>
        [Fact]
        public async Task GetContractById_WhenExists_ReturnOK()
        {
            // Arrange
            // Vai buscar todos os contratos e usa o primeiro Id.
            var allContracts = await _httpClient.GetFromJsonAsync<IEnumerable<ContractApiViewModel>>("/api/v1/contract");
            var contractId = allContracts!.First().Id;

            // Act
            // Faz GET do endpoint
            var response = await _httpClient.GetAsync($"/api/v1/contract/{contractId}");

            // Assert
            //Verifica se a resposta é OK
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        /// Não devolve contract quando o Id não existe.
        /// </summary>
        /// <returns>Devolv NotFound, Id não existe</returns>
        [Fact]
        public async Task GetContractById_WhenDoesNotExist_ReturnNotFound()
        {
            // Arrange
            // Criar um Guid para uma nova order
            var invalidId = Guid.Parse("aaaaaaaa-aaaa-aaaa-bccb-aaaaaaaaaaaa");

            // Act
            // Faz GET do endpoint
            var response = await _httpClient.GetAsync($"/api/v1/contract/{invalidId}");

            // Assert
            //Verifica se a resposta é Not Found
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Tenta atualizar contract mas Id diferente
        /// </summary>
        /// <returns>Devolve BadRequest, id diferente</returns>
        [Fact]
        public async Task UpdateContract_WhenIdMismatch_ReturnBadRequest()
        {
            // Arrange
            // Vai buscar todas as contract da bd InMemory
            var allContracts = await _httpClient.GetFromJsonAsync<IEnumerable<ContractApiViewModel>>("/api/v1/contract");
            var contract = allContracts!.First();


            var differentId = Guid.NewGuid();

            // Act
            // Faz PUT do endpoint
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/contract/{differentId}", contract);

            // Assert
            //Verifica se a resposta é BadRequest
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }

        /// <summary>
        /// Atualizar um contract existente com termos válidos.
        /// </summary>
        /// <returns>Devovle OK, contract validos</returns>
        [Fact]
        public async Task UpdateContract_WhenValid_ReturnOK()
        {
            // Arrange
            // Vai buscar todas as contract da bd InMemory
            var allContracts = await _httpClient.GetFromJsonAsync<IEnumerable<ContractApiViewModel>>("/api/v1/contract");
            var contract = allContracts!.First();

            contract.TermsConditions = "Novos termos de teste para o contrato.";

            // Act
            // Faz PUT do endpoint
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/contract/{contract.Id}", contract);


            // Assert
            // Verifica se a resposta é OK
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        /// Não remove contract quando o Id não existe.
        /// </summary>
        /// <returns>Devolve NotFound, Id não existe</returns>
        [Fact]
        public async Task DeleteContract_WhenIdDoesNotExist_ReturnNotFound()
        {
            // Arrange
            // Vai buscar todas as contract da bd InMemory
            var invalidId = Guid.Parse("aaaaaaaa-adda-abca-aaaa-aaaaaaaaaaaa");

            // Act
            // Faz DELETE do endpoint
            var response = await _httpClient.DeleteAsync($"/api/v1/contract/{invalidId}");

            // Assert
            // Verifica se a resposta é Not Found
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
        }

        /// <summary>
        /// Remover um contract existente.
        /// </summary>
        /// <returns>Devolve OK, apaga contract</returns>
        [Fact]
        public async Task DeleteContract_WhenExists_ReturnOK()
        {
            // Arrange
            // Vai buscar todas as contract da bd InMemory
            var allContracts = await _httpClient.GetFromJsonAsync<IEnumerable<ContractApiViewModel>>("/api/v1/contract");
            var contract = allContracts!.First();

            // Act
            // Faz DELETE do endpoint
            var response = await _httpClient.DeleteAsync($"/api/v1/contract/{contract.Id}");

            // Assert
            // Verifica se a resposta é Ok
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }
    }
}
