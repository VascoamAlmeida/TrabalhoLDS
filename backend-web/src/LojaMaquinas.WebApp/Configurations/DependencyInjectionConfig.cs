using LojaMaquinas.Business.Interfaces;
using LojaMaquinas.Business.Interfaces.Entities;
using LojaMaquinas.Business.Interfaces.Parameters;
using LojaMaquinas.Business.Services;
using LojaMaquinas.Business.Services.Auxiliaries;
using LojaMaquinas.Business.Services.Entities;
using LojaMaquinas.Business.Services.Parameters;
using LojaMaquinas.Data.Contexts;
using LojaMaquinas.Data.Repositories.Entities;
using LojaMaquinas.Data.Repositories.Parameters;

namespace LojaMaquinas.WebApp.Configurations
{
    /// <summary>
    /// Configuração centralizada de injeção de dependências da aplicação.
    /// Regista todos os serviços, repositórios e contexto de base de dados no contentor IoC.
    /// </summary>
    public static class DependencyInjectionConfig
    {
        /// <summary>
        /// Método de extensão que resolve todas as dependências da aplicação.
        /// Regista com ciclo de vida Scoped (uma instância por request HTTP):
        /// - Contexto da base de dados (MyDbContext)
        /// - Serviços gerais (NotifierService)
        /// - Serviços de domínio (Category, Contract, Order, Product, Stock, Client, User, Maintenance, OrderRent, Terms)
        /// - Repositórios de dados (acesso à base de dados)
        /// - Serviços auxiliares (PdfService)
        /// </summary>
        /// <param name="services">Coleção de serviços do ASP.NET Core</param>
        /// <returns>Coleção de serviços com dependências registadas</returns>
        public static IServiceCollection ResolveDependencies(this IServiceCollection services)
        {
            /* General Services */
            services.AddScoped<MyDbContext>();
            services.AddScoped<INotifierService, NotifierService>();

            /* Services */
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IContractService, ContractService>();
            services.AddScoped<IOrderRentService, OrderRentService>();
            services.AddScoped<ITermsService, TermsService>();
            services.AddScoped<PdfService>();

            /* Repositories */
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IContractRepository, ContractRepository>();
            services.AddScoped<IOrderRentRepository, OrderRentRepository>();

            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IStockService, StockService>();
            services.AddScoped<IClientService, ClientService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IMaintenanceService, MaintenanceService>();

            /* Repositories */
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IStockRepository, StockRepository>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();

            return services;
        }
    }
}
