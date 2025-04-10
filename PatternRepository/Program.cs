
using Application.Services;
using Infrastructure.RepositoryPattern.Infrastructure;
using Infrastructure;
using Domain;
using Infrastructure;
using Application;

namespace API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();

            // Add Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "Repository Pattern Demo API", Version = "v1" });
            });

            // Register Application Services
            builder.Services.AddScoped<UserService>();

            // Register Infrastructure
            object value = builder.Services.AddInfrastructure(builder.Configuration); // Remova a atribui��o a 'object value'

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            // Inicializa��o do banco de dados
            using (var scope = app.Services.CreateScope())
            {
                var dbInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
                await dbInitializer.InitializeAsync();
            }

            app.Run();
        }
    }
}
