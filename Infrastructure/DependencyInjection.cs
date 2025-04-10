using Domain.Repositories;
using Infrastructure.RepositoryPattern.Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Registrar a conexão SQL
            services.AddScoped(_ => new SqlConnection(configuration.GetConnectionString("DefaultConnection")));

            // Registrar os repositórios
            services.AddScoped<IUserRepository, Repositories.SqlUserRepository>();

            // Registrar o serviço de inicialização do banco de dados
            services.AddScoped<DatabaseInitializer>();

            return services;
        }
    }
}
