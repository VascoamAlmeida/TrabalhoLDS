using LojaMaquinas.Data.Contexts;
using LojaMaquinas.WebApp;
using LojaMaquinas.WebApp.Configurations;
using Microsoft.AspNetCore.Authentication.JwtBearer; //Installar microsoft.aspnetCore.authentication.JwtBearer version 8
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text; // Encoding.UTF8.GetBytes

var builder = WebApplication.CreateBuilder(args);

// Serve para ler valores do appsetting.json
var config = builder.Configuration;

// Configuração da base de dados
if (!builder.Environment.IsEnvironment("Testing"))
{
    var useSqlite = builder.Configuration.GetValue<bool>("UseSQLite", false);

    if (useSqlite)
    {
        var sqliteConnection = builder.Configuration.GetConnectionString("SQLiteConnection")
            ?? "Data Source=lojamaquinas.db";

        builder.Services.AddDbContext<MyDbContext>(options =>
            options.UseSqlite(sqliteConnection));
    }
    else
    {
        var sqlServerConnection = builder.Configuration.GetConnectionString("LojaMaquinas");
        if (string.IsNullOrWhiteSpace(sqlServerConnection))
        {
            throw new InvalidOperationException(
                "ConnectionStrings:LojaMaquinas is required when UseSQLite is false. " +
                "Provide it through environment variables, user-secrets or another secure configuration provider.");
        }

        builder.Services.AddDbContext<MyDbContext>(options =>
            options.UseSqlServer(sqlServerConnection));
    }
}

// Auto Mapper
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// Injeção de dependências customizadas
builder.Services.ResolveDependencies();

builder.Services.AddControllersWithViews(); 
builder.Services.AddEndpointsApiExplorer();


// Gera o Swagger
builder.Services.AddSwaggerGen(c =>
{
    //Cria documento Swagger v1
    c.SwaggerDoc("v1", new OpenApiInfo
        {
        Title = "Loja de Máquinas API",
        Version = "v1",
        Description = "API para gestão de vendas, alugueres, manutenção e catálogo.",
        });

    // Configure Swagger to use Bearer tokens
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
        });

    // Faz com que todos endpoints usem o Bearer
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
});


var jwtKey = config["JwtSettings:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "JwtSettings:Key must be configured with at least 32 bytes. " +
        "Use an environment variable or user-secrets; do not commit signing keys.");
}

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "Cookies";
        options.DefaultChallengeScheme = "Cookies";
    })
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.AccessDeniedPath = "/access-denied";
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
        options.SlidingExpiration = true;
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, x =>
    {
        x.TokenValidationParameters = new TokenValidationParameters
            {
            ValidIssuer = config["JwtSettings:Issuer"],
            ValidAudience = config["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
            };
    });


builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("ADMIN"))
    .AddPolicy("ClientOnly", policy => policy.RequireRole("CLIENT"));
var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AppCors", policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

builder.Services.AddSingleton<TokenGenerator>();

// Background Services
builder.Services.AddHostedService<LojaMaquinas.WebApp.BackgroundServices.MaintenanceStatusUpdateService>();

var app = builder.Build();

// Ensure database is created (SQLite auto-creates on first use)
using (var scope = app.Services.CreateScope())
    {
    var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    db.Database.EnsureCreated();
    }

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
    {
    app.UseDeveloperExceptionPage(); // mostra detalhes de erro durante o desenvolvimento
    }
else
    {
    app.UseExceptionHandler("/erro/500"); // Página de erro em produção
    app.UseStatusCodePagesWithReExecute("/erro/{0}"); // Reexecuta a rota de erro sem criar loops
    app.UseHsts();
    }

// Enable Swagger in all environments
app.UseSwagger(); // expõe /swagger/v1/swagger.json
app.UseSwaggerUI(c =>  // UI interativa em /swagger
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "LojaMaquinas V1");
});


// Middleware padrão
// Em ambientes não-Development, redireciona para HTTPS.
if (!app.Environment.IsDevelopment())
    {
    app.UseHttpsRedirection();
    }
app.UseStaticFiles();
app.UseRouting();

app.UseCors("AppCors");
// Middlewares de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();


// Mapeamento de endpoints
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


// Garante mapeamento de APIs por atributos
app.MapControllers();

// Arranca servidor
app.Run();

public partial class Program { }
