using FluentAssertions;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.WebApp.ViewModels.API;
using System.Net.Http.Json;

namespace IntegrationTests.ApiTests
{
    /// <summary>
    /// Classe de testes de integração para a API de encomendas (OrderApi).
    /// Testa os endpoints HTTP (GET, POST, PUT, PATCH, DELETE) relativos às encomendas.
    /// </summary>
    public class OrderApiTests : IClassFixture<IntegrationTestWebAppFactory>
    {
        /// <summary>
        /// Factory responsável por criar a aplicação web em ambiente de teste.
        /// </summary>
        private readonly IntegrationTestWebAppFactory _factory;
        /// <summary>
        /// Cliente HTTP utilizado para efetuar os pedidos à API.
        /// </summary>
        private readonly HttpClient _httpClient;

        /// <summary>
        /// Construtor que inicializa o cliente HTTP e o factory para os testes.
        /// </summary>
        public OrderApiTests(IntegrationTestWebAppFactory factory)
        {
            _factory = factory;
            _httpClient = _factory.CreateAuthenticatedClient();
            _factory.ResetDatabase();
        }

        /// <summary>
        /// Testa a obtenção de todas as encomendas.
        /// Deve retornar o código HTTP 200 (OK).
        /// </summary>
        [Fact]
        public async Task GetAllOrders_ReturnOK() 
        {
            // Arrange
            // Nao preciso de nada, já tenho da minha seeding

            // Act
            // Faz GET do endpoint
            var response = await _httpClient.GetAsync("/api/v1/OrderApi");

            // Assert
            //Verifica se a resposta é OK
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        }

        /// <summary>
        /// Testa a obtenção de uma encomenda existente por ID.
        /// Deve retornar o código HTTP 200 (OK).
        /// </summary>
        [Fact]
        public async Task GetOrderById_WhenExists_ReturnOK()
        {
            // Arrange
            // Criar um Guid para uma nova order
            var orderId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

            // Act
            // Faz GET do endpoint
            var response = await _httpClient.GetAsync($"/api/v1/OrderApi/{orderId}");

            // Assert
            //Verifica se a resposta é OK
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        /// Testa a obtenção de uma encomenda inexistente por ID.
        /// Deve retornar o código HTTP 404 (NotFound).
        /// </summary>
        [Fact]
        public async Task GetOrderById_WhenDoesNotExist_ReturnNotFound()
        {
            // Arrange
            // Criar um Guid para uma nova order
            var orderId = Guid.Parse("aaaaaaaa-aaaa-aaaa-bbbb-aaaaaaaaaaaa");

            // Act
            // Faz GET do endpoint
            var response = await _httpClient.GetAsync($"/api/v1/OrderApi/{orderId}");

            // Assert
            //Verifica se a resposta é Not Found
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

        }

        /// <summary>
        /// Testa a obtenção das encomendas de um cliente existente.
        /// Deve retornar o código HTTP 200 (OK).
        /// </summary>
        [Fact]
        public async Task GetOrdersByClientId_WhenClientExists_ReturnOK()
        {
			// Arrange
			// Usar ClientId da seed para ir buscar as suas Orders
			var allOrders = await _httpClient.GetFromJsonAsync<IEnumerable<OrderApiViewModel>>("/api/v1/OrderApi");
			var clientId = allOrders!.First().ClientId;

			// Act
			// Faz GET do endpoint
			var response = await _httpClient.GetAsync($"/api/v1/OrderApi/client/{clientId}");// Tem de se mudar caminho deste NAO?

			// Assert
			//Verifica se a resposta é OK
			response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        /// Testa a criação de uma nova encomenda válida.
        /// Deve retornar o código HTTP 200 (OK).
        /// </summary>
        [Fact]
        public async Task CreateOrder_WhenValid_ReturnOK()
        {
            // Arrange
            // Vai buscar todas as orders da bd InMemory
            var seeded = await _httpClient.GetFromJsonAsync<IEnumerable<OrderApiViewModel>>("/api/v1/OrderApi");
            var clientId = seeded.First().ClientId;
            var stockId = seeded.First().StockId;

            // Faz uma nova order para criar
            var newOrder = new OrderApiViewModel()
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                StockId = stockId,
                OrderDate = DateTime.UtcNow,
                Total = 200,
                Type = OrderType.Sale,
                Status = OrderStatus.Active
            };

            // Act
            // Faz POST do endpoint
            var response = await _httpClient.PostAsJsonAsync("/api/v1/OrderApi", newOrder);

            // Assert
            //Verifica se a resposta é OK
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        /// Testa a criação de uma encomenda com um StockId inexistente.
        /// Deve retornar o código HTTP 500 (InternalServerError).
        /// </summary>
        [Fact]
        public async Task CreateOrder_WhenStockDoesNotExist_ReturnInternalServerError()
        {
            // Arrange
            // Vai buscar todas as orders da bd InMemory
            var seeded = await _httpClient.GetFromJsonAsync<IEnumerable<OrderApiViewModel>>("/api/v1/OrderApi");
            var clientId = seeded.First().ClientId;
            // Criar um Guid de stock
            var stockId = Guid.Parse("aaaaaaaa-eeee-aaaa-aaaa-aaaaaaaaaaaa");

            var newOrder = new OrderApiViewModel()
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                StockId = stockId,
                OrderDate = DateTime.UtcNow,
                Total = 200,
                Type = OrderType.Sale,
                Status = OrderStatus.Active
            };

            // Act
            // Faz POST do endpoint
            var response = await _httpClient.PostAsJsonAsync("/api/v1/OrderApi", newOrder);

            // Assert
            //Verifica se a resposta é Internal Server Error
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Testa a criação de uma encomenda com um ClientId inexistente.
        /// Deve retornar o código HTTP 500 (InternalServerError).
        /// </summary>
        [Fact]
        public async Task CreateOrder_WhenClientDoesNotExist_ReturnInternalServerError()
        {
            // Arrange
            // Vai buscar todas as orders da bd InMemory
            var seeded = await _httpClient.GetFromJsonAsync<IEnumerable<OrderApiViewModel>>("/api/v1/OrderApi");
            // Criar um Guid de client
            var clientId = Guid.Parse("aaaaaaaa-ffff-aaaa-aaaa-aaaaaaaaaaaa");
            var stockId = seeded.First().StockId;

            var newOrder = new OrderApiViewModel()
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                StockId = stockId,
                OrderDate = DateTime.UtcNow,
                Total = 200,
                Type = OrderType.Sale,
                Status = OrderStatus.Active
            };

            // Act
            // Faz POST do endpoint
            var response = await _httpClient.PostAsJsonAsync("/api/v1/OrderApi", newOrder);

            // Assert
            //Verifica se a resposta é Internal Server Error
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.InternalServerError);
        }

        /// <summary>
        /// Testa a atualização de uma encomenda válida.
        /// Deve retornar o código HTTP 200 (OK).
        /// </summary>
        [Fact]
        public async Task UpdateOrder_WhenValid_ReturnOK()
        {
            // Arrange
            // Vai buscar todas as orders da bd InMemory
            var seedOrder = (await _httpClient.GetFromJsonAsync<IEnumerable<OrderApiViewModel>>("/api/v1/OrderApi"));
            var order = seedOrder!.First(); 

            // Que estou a mudar
            order.Total += 300;

            // Act
            // Faz PUT do endpoint
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/OrderApi/{order.Id}", order);

            // Assert
            //Verifica se a resposta é OK
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        /// Testa o cancelamento de uma encomenda ativa.
        /// Deve retornar o código HTTP 200 (OK).
        /// </summary>
        [Fact]
        public async Task CancelOrder_WhenActive_ReturnOK()
        {
            // Arrange
            // Vai buscar todas as orders da bd InMemory
            var seedOrder = await _httpClient.GetFromJsonAsync<List<OrderApiViewModel>>("/api/v1/OrderApi");
            // Escolhe o Id de uma order com estado ativo 
            var activeId = seedOrder!.First(o => o.Status == OrderStatus.Active).Id;

            // Act
            // Faz PATCH do endpoint
            var response = await _httpClient.PatchAsJsonAsync($"/api/v1/OrderApi/{activeId}/cancel", seedOrder);

            // Assert
            //Verifica se a resposta é OK
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        /// Testa a conclusão de uma encomenda ativa.
        /// Deve retornar o código HTTP 200 (OK).
        /// </summary>
        [Fact]
        public async Task CompleteOrder_WhenActive_ReturnOK()
        {
            var seedOrders = await _httpClient.GetFromJsonAsync<List<OrderApiViewModel>>("/api/v1/OrderApi");
            var activeId = seedOrders!.First(o => o.Status == OrderStatus.Active).Id;

            var response = await _httpClient.PatchAsJsonAsync($"/api/v1/OrderApi/{activeId}/complete", new { });

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        /// <summary>
        /// Testa a tentativa de eliminar uma encomenda inexistente.
        /// Deve retornar o código HTTP 404 (NotFound).
        /// </summary>
        [Fact]
        public async Task DeleteOrder_WhenIdDoesNotExist_ReturnNotFound()
        {
            // Arrange
            // Criar um Guid para estado cancelado
            var cancelledId = Guid.Parse("aaaaaaaa-adda-aaaa-aaaa-aaaaaaaaaaaa");

            // Act
            // Faz DELETE do endpoint
            var response = await _httpClient.DeleteAsync($"/api/v1/OrderApi/{cancelledId}");

            // Assert
            //Verifica se a resposta é Not Found
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);

        }

        /// <summary>
        /// Testa a eliminação de uma encomenda com estado Completed.
        /// Deve retornar o código HTTP 400 (BadRequest).
        /// </summary>
        [Fact]
        public async Task DeleteOrder_WhenCompleted_ReturnBadRequest()
        {
            // Arrange
            // Vai buscar todas as orders da bd InMemory
            var seedOrder = await _httpClient.GetFromJsonAsync<List<OrderApiViewModel>>("/api/v1/OrderApi");
            // Escolhe o Id de uma order com estado completed 
            var completedId = seedOrder!.First(o => o.Status == OrderStatus.Completed).Id;

            // Act
            // Faz DELETE do endpoint
            var response = await _httpClient.DeleteAsync($"/api/v1/OrderApi/{completedId}");

            // Assert
            //Verifica se a resposta é Bad Request
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        }

        /// <summary>
        /// Testa o download de uma encomenda em formato PDF pelo seu ID.
        /// Deve retornar um ficheiro PDF (200 OK).
        /// </summary>
        [Fact]
        public async Task DownloadOrderPdf_ReturnPdfByOrderId()
        {
            // Arrange
            // Criar um Guid para uma nova order
            var orderId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

            // Act
            var response = await _httpClient.GetAsync($"/api/v1/OrderApi/{orderId}/download-pdf");

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");

        }

        [Fact]
        public async Task GetAllOrders_WhenClient_ReturnForbidden()
        {
            var clientHttp = _factory.CreateAuthenticatedClient(role: "CLIENT", userId: Guid.NewGuid());

            var response = await clientHttp.GetAsync("/api/v1/OrderApi");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetOrderById_WhenClientDoesNotOwnOrder_ReturnForbidden()
        {
            var clientHttp = _factory.CreateAuthenticatedClient(role: "CLIENT", userId: Guid.NewGuid());
            var orderId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

            var response = await clientHttp.GetAsync($"/api/v1/OrderApi/{orderId}");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task ActiveOrder_WhenAlreadyActive_ReturnBadRequest()
        {
            var seedOrders = await _httpClient.GetFromJsonAsync<List<OrderApiViewModel>>("/api/v1/OrderApi");
            var activeId = seedOrders!.First(o => o.Status == OrderStatus.Active).Id;

            var response = await _httpClient.PatchAsJsonAsync($"/api/v1/OrderApi/{activeId}/active", new { });

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }
}
