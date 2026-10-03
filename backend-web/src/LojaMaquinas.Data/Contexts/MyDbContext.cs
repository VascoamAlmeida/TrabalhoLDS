using LojaMaquinas.Business.Models.Entities;
using LojaMaquinas.Business.Models.Parameters;
using Microsoft.EntityFrameworkCore;

namespace LojaMaquinas.Data.Contexts
{
    /// <summary>
    /// Contexto principal da base de dados da aplicação LojaMaquinas.
    /// Gerencia todas as entidades e configurações do Entity Framework Core.
    /// </summary>
    public class MyDbContext : DbContext
    {
        /// <summary>
        /// Inicializa uma nova instância do contexto da base de dados.
        /// Configura o ChangeTracker para otimizar performance em cenários read-only:
        /// - NoTracking por defeito para queries (melhor performance)
        /// - AutoDetectChanges desativado (controlo manual de mudanças)
        /// </summary>
        /// <param name="options">Opções de configuração do DbContext (connection string, provider, etc.)</param>
        public MyDbContext(DbContextOptions options) : base(options)
        {
            ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            ChangeTracker.AutoDetectChangesEnabled = false;
        }

        /// <summary>
        /// Conjunto de entidades de Produtos na base de dados.
        /// Representa a tabela E_Products.
        /// </summary>
        public DbSet<Product> E_Products { get; set; }

        /// <summary>
        /// Conjunto de parâmetros de Categorias na base de dados.
        /// Representa a tabela P_Categories.
        /// </summary>
        public DbSet<Category> P_Categories { get; set; }

        /// <summary>
        /// Conjunto de entidades de Stock/Inventário na base de dados.
        /// Representa a tabela E_Stocks.
        /// </summary>
        public DbSet<Stock> E_Stocks { get; set; }

        /// <summary>
        /// Conjunto de entidades de Utilizadores na base de dados.
        /// Representa a tabela E_Users.
        /// </summary>
        public DbSet<User> E_Users { get; set; }

        /// <summary>
        /// Conjunto de entidades de Clientes na base de dados.
        /// Representa a tabela E_Clients.
        /// </summary>
        public DbSet<Client> E_Clients { get; set; }

        /// <summary>
        /// Conjunto de entidades de Manutenções na base de dados.
        /// Representa a tabela E_Maintenances.
        /// </summary>
        public DbSet<Maintenance> E_Maintenances { get; set; }

        /// <summary>
        /// Conjunto de entidades de Pedidos na base de dados.
        /// Representa a tabela E_Orders.
        /// </summary>
        public DbSet<Order> E_Orders { get; set; }

        /// <summary>
        /// Conjunto de entidades de Alugueres (Pedidos de Aluguer) na base de dados.
        /// Representa a tabela E_OrderRents.
        /// </summary>
        public DbSet<OrderRent> E_OrderRents { get; set; }

        /// <summary>
        /// Conjunto de entidades de Contratos na base de dados.
        /// Representa a tabela E_Contracts.
        /// </summary>
        public DbSet<Contract> E_Contracts { get; set; }

        /// <summary>
        /// Configura o modelo da base de dados durante a criação.
        /// Aplica convenções globais:
        /// - Define varchar(100) como tipo padrão para todas as propriedades string
        /// - Aplica todas as configurações IEntityTypeConfiguration do assembly
        /// - Define ClientSetNull como comportamento padrão para delete de relacionamentos (previne cascade delete)
        /// </summary>
        /// <param name="modelBuilder">Construtor do modelo que permite configurar as entidades</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var property in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetProperties().Where(p => p.ClrType == typeof(string))))
                property.SetColumnType("varchar(100)");

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyDbContext).Assembly);

            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys())) relationship.DeleteBehavior = DeleteBehavior.ClientSetNull;

            base.OnModelCreating(modelBuilder);
        }

        /// <summary>
        /// Grava as alterações na base de dados de forma assíncrona.
        /// Atualiza automaticamente a propriedade UpdateDate das entidades modificadas:
        /// - Entidades novas (Added): UpdateDate não é modificado
        /// - Entidades alteradas (Modified): UpdateDate é atualizado com DateTime.Now
        /// </summary>
        /// <param name="cancellationToken">Token de cancelamento da operação assíncrona</param>
        /// <returns>Número de entidades afetadas na base de dados</returns>
        public async override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            foreach (var entry in ChangeTracker.Entries().Where(entry => entry.Entity.GetType().GetProperty("UpdateDate") != null)){
                if (entry.State == EntityState.Added)
                    entry.Property("UpdateDate").IsModified = false;

                if (entry.State == EntityState.Modified)
                    entry.Property("UpdateDate").CurrentValue = DateTime.Now;
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
