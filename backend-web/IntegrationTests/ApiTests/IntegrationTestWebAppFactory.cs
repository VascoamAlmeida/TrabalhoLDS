using LojaMaquinas.Data.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.Data.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;

namespace IntegrationTests.ApiTests
{
    /// <summary>
    /// Factory personalizada para testes de integração da aplicação web.
    /// Configura um ambiente de teste com base de dados SQLite in-memory.
    /// </summary>
    public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
    {
        private const string TestJwtKey = "TEST_ONLY_JWT_SIGNING_KEY_12345678901234567890";
        /// <summary>
        /// Conexão SQLite in-memory mantida aberta durante o tempo de vida dos testes.
        /// </summary>
        private SqliteConnection _connection;


        /// <summary>
        /// Configura o host web para testes, substituindo a base de dados por uma SQLite in-memory.
        /// </summary>
        /// <param name="builder">Builder do host web</param>
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["JwtSettings:Key"] = TestJwtKey,
                    ["JwtSettings:Issuer"] = "LojaMaquinas-API",
                    ["JwtSettings:Audience"] = "LojaMaquinas-Clients",
                    ["JwtSettings:ExpireMinutes"] = "60"
                });
            });

            builder.ConfigureTestServices(services =>
            {
                //// Remove a BD original 
                //var descriptor = services
                //.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<MyDbContext>));

                //if(descriptor != null)
                //{
                //    services.Remove(descriptor);
                //}

                //// Remove connection anterior se exisitir
                //var descriptorConnection = services.SingleOrDefault(d => d.ServiceType == typeof(DbConnection));

                //if (descriptorConnection != null)
                //{
                //    services.Remove(descriptorConnection);
                //}

                // Adiciona nova ligaçao ao SQLite InMemory
                //services.AddSingleton<DbConnection>(container =>
                //{
                //    var connection = new SqliteConnection("DataSource=:memory:");
                //    connection.Open();

                //    return connection;
                //}
                //);

                // Remove os registos do MyDbContext que o app tenha feito
                services.RemoveAll(typeof(DbContextOptions<MyDbContext>));
                services.RemoveAll(typeof(MyDbContext));
                services.RemoveAll(typeof(DbConnection));

                // Adiciona nova ligaçao ao SQLite InMemory
                _connection = new SqliteConnection("DataSource=:memory:");
                _connection.Open();

                services.AddSingleton<DbConnection>(_ => _connection);


                // Serve para utilizar a DB inMemory
                services.AddDbContext<MyDbContext>((container, options) =>
                {
                    var connection = container.GetRequiredService<DbConnection>();
                    options.UseSqlite(connection);
                });

                // Serve para dar os testes a seed da BD InMemory
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

                db.Database.EnsureCreated(); // cria tabelas
                Seeding.InitializeTestDB(db); // faz seed

            });
        }

        /// <summary>
        /// Liberta os recursos utilizados pela factory, incluindo a conexão à base de dados in-memory.
        /// </summary>
        /// <param name="disposing">Indica se os recursos geridos devem ser libertados</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            // Fecha a ligação quando a factory for destruída
            _connection?.Dispose();
        }

        /// <summary>
        /// Repõe a base de dados de testes no estado inicial para manter os testes independentes.
        /// </summary>
        public void ResetDatabase()
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            Seeding.InitializeTestDB(db);
        }

        /// <summary>
        /// Cria um HttpClient autenticado com token JWT
        /// </summary>
        /// <param name="email">Email do usuário (default: admin@test.com)</param>
        /// <param name="role">Role do usuário (default: Admin)</param>
        /// <returns>HttpClient configurado com autenticação</returns>
        public HttpClient CreateAuthenticatedClient(string email = "admin@test.com", string role = "ADMIN", Guid? userId = null)
        {
            var client = CreateClient(new()
            {
                AllowAutoRedirect = false
            });

            var token = GenerateTestToken(email, role, userId);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        /// <summary>
        /// Gera um token JWT válido para testes
        /// </summary>
        private string GenerateTestToken(string email, string role, Guid? userId = null)
        {
            var authenticatedUserId = userId ?? Guid.NewGuid();
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, email),
                new(JwtRegisteredClaimNames.Email, email),
                new(ClaimTypes.NameIdentifier, authenticatedUserId.ToString()),
                new("userId", authenticatedUserId.ToString()),
                new(ClaimTypes.Role, role)
            };

            // Mesmas configurações do appsettings.json
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: "LojaMaquinas-API",
                audience: "LojaMaquinas-Clients",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
