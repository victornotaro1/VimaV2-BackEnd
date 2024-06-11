using VimaV2.Models;
using VimaV2.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace VimaV2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // Criação da WebApplication
            var builder = WebApplication.CreateBuilder(args);

            // Configuração do Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Configuração do Banco de Dados
            builder.Services.AddDbContext<VimaV2DbContext>(options =>
                options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"), ServerVersion.Parse("8.0.37-mysql")));

            builder.Services.AddScoped<VimaV2DbContext>();

            // Add services to the container.
            builder.Services.AddControllers();

            // Configuração do CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors("AllowAllOrigins"); // Adicione isso antes de UseAuthorization

            app.UseAuthorization();

            app.MapControllers();

            #region Users
            // Rotas de usuários
            app.MapGet("/usuarios", (VimaV2DbContext dbContext) =>
            {
                return Results.Ok(dbContext.Usuarios);
            });

            app.MapPost("/usuario", (VimaV2DbContext dbContext, User user) =>
            {
                dbContext.Usuarios.Add(user);
                dbContext.SaveChanges();
                return Results.Created($"/usuario/{user.Id}", user);
            });
            #endregion

            #region Contact
            // Rotas de contato
            app.MapGet("/contact", (VimaV2DbContext dbContext) =>
            {
                return Results.Ok(dbContext.Contatos);
            });

            app.MapPost("/contact", (Contato contato, VimaV2DbContext dbContext) =>
            {
                dbContext.Contatos.Add(contato);
                dbContext.SaveChanges();
                return Results.Created($"/contact/{contato.Id}", contato);
            });
            #endregion

            #region Produto
            // Rotas de produto
            app.MapGet("/produtos", async (VimaV2DbContext dbContext) =>
            {
                var produtos = await dbContext.Produtos.ToListAsync();
                return Results.Ok(produtos);
            });

            app.MapPost("/produto", async (VimaV2DbContext dbContext, Produto produto) =>
            {
                dbContext.Produtos.Add(produto);
                await dbContext.SaveChangesAsync();
                return Results.Created($"/produto/{produto.Id}", produto);
            });

            app.MapGet("/produto/{id}", async (VimaV2DbContext dbContext, int id) =>
            {
                var produto = await dbContext.Produtos.FindAsync(id);
                if (produto == null)
                {
                    return Results.NotFound();
                }
                return Results.Ok(produto);
            });
            #endregion

            // Execução da aplicação
            app.Run();
        }
    }
}
