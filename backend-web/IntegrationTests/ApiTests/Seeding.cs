using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Enums;
using LojaMaquinas.Business.Models.Parameters;
using LojaMaquinas.Data.Contexts;

namespace IntegrationTests.ApiTests
{
    /// <summary>
    /// Classe responsável por popular a base de dados de testes com dados iniciais.
    /// </summary>
    public class Seeding
    {
        /// <summary>
        /// Inicializa a base de dados de testes com dados pré-definidos.
        /// Remove todos os dados existentes e adiciona entidades de teste (utilizadores, clientes, produtos, pedidos, etc.).
        /// </summary>
        /// <param name="db">Contexto da base de dados</param>
        public static void InitializeTestDB(MyDbContext db)
        {
            // Remove todos os dados existentes da base de dados in-memory
            db.E_Orders.RemoveRange(db.E_Orders);
            db.E_Clients.RemoveRange(db.E_Clients);
            db.P_Categories.RemoveRange(db.P_Categories);
            db.E_Contracts.RemoveRange(db.E_Contracts);
            db.E_Maintenances.RemoveRange(db.E_Maintenances);
            db.E_OrderRents.RemoveRange(db.E_OrderRents);
            db.E_Products.RemoveRange(db.E_Products);
            db.E_Stocks.RemoveRange(db.E_Stocks);
            db.E_Users.RemoveRange(db.E_Users);
            db.SaveChanges();


            var userId = Guid.NewGuid();
            var user = new User
            {
                Id = userId,
                Email = "admin@mail.com",
                Password = "Password123!",
                UserType = "ADMIN",
                IsActive = true
            };

            var clientId = Guid.NewGuid();
            var client = new Client
            {
                Id = clientId,
                UserId = userId,
                FirstName = "Client",
                LastName = "Client",
                Address = "Client Street",
                PostalCode = "1234-567",
                PhoneNumber = "911111111",
                NIF = "123456789"
            };

            // PRODUCT + STOCK
            var category = new Category { Id = Guid.NewGuid(), Name = "Machines", Description = "Maquina de produçao" };

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = "Cerim",
                Type = ProductType.Machine,
                Brand = "Siemens",
                Model = "X1000",
                YearManufacture = 2024,
                Description = "Maquina produção",
                Price = 25000,
                Quantity = 10,
                Image = "img.png",
                CategoryId = category.Id,
                IsActive = true
            };

            var stock = new Stock
            {
                Id = Guid.NewGuid(),
                Ref = Guid.NewGuid(),
                ProductId = product.Id,
                StockStatus = StockStatus.Available
            };

            // ORDER 1
            var orderId1 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var order1 = new Order
            {
                Id = orderId1,
                ClientId = clientId,
                StockId = stock.Id,
                OrderDate = DateTime.UtcNow,
                Total = 100,
                Type = OrderType.Rent,
                Status = OrderStatus.Active
            };

            // ORDER 2
            var orderId2 = Guid.Parse("aaaaaaaa-bbbb-aaaa-aaaa-aaaaaaaaaaaa");
            var order2 = new Order
            {
                Id = orderId2,
                ClientId = clientId,
                StockId = stock.Id,
                OrderDate = DateTime.UtcNow,
                Total = 100,
                Type = OrderType.Rent,
                Status = OrderStatus.Cancelled
            };

            // ORDER 3
            var orderId3 = Guid.Parse("aaaaaaaa-cccc-aaaa-aaaa-aaaaaaaaaaaa");
            var order3 = new Order
            {
                Id = orderId3,
                ClientId = clientId,
                StockId = stock.Id,
                OrderDate = DateTime.UtcNow,
                Total = 100,
                Type = OrderType.Rent,
                Status = OrderStatus.Completed
            };

            // ORDER RENT + CONTRACT
            var orderRent = new OrderRent
            {
                Id = Guid.NewGuid(),
                OrderId = order1.Id,
                InitialDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(7),
                CompanyImage = "company.png",
                Status = OrderRentStatus.Active
            };

            var contract = new Contract
            {
                Id = Guid.NewGuid(),
                OrderRentId = orderRent.Id,
                TermsConditions = "Termos de exemplo"
            };

            // ORDER RENT + CONTRACT
            var orderRent2 = new OrderRent
            {
                Id = Guid.NewGuid(),
                OrderId = order2.Id,
                InitialDate = DateTime.UtcNow.AddDays(-1),
                EndDate = DateTime.UtcNow.AddDays(7),
                CompanyImage = "company.png",
                Status = OrderRentStatus.Active
            };

            var contract2 = new Contract
            {
                Id = Guid.NewGuid(),
                OrderRentId = orderRent2.Id,
                TermsConditions = "Termos de exemplo"
            };

            // MAINTENANCE
            var maintenance = new Maintenance
            {
                Id = Guid.NewGuid(),
                OrderId = order1.Id,
                ScheduledDate = DateTime.UtcNow.Date.AddDays(14),
                Price = 450.00m,
                Obs = "Initial scheduled maintenance for machine.",
                Status = MaintenanceStatus.Pending
            };

            // Vai adicionar a nova db InMemory 
            db.E_Users.Add(user);
            db.E_Clients.Add(client);
            db.P_Categories.Add(category);
            db.E_Products.Add(product);
            db.E_Stocks.Add(stock);
            db.E_Orders.Add(order1);
            db.E_Orders.Add(order2);
            db.E_Orders.Add(order3);
            db.E_OrderRents.Add(orderRent);
            db.E_Contracts.Add(contract);
            db.E_Maintenances.Add(maintenance);
            db.E_OrderRents.Add(orderRent2);
            db.E_Contracts.Add(contract2);

            db.SaveChanges();
        }
    }
}