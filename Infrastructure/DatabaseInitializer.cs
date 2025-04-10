using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Entities;

using Microsoft.Data.SqlClient;
using System.Data;

namespace Infrastructure
{


    namespace RepositoryPattern.Infrastructure
    {
        public class DatabaseInitializer
        {
            private readonly SqlConnection _connection;

            public DatabaseInitializer(SqlConnection connection)
            {
                _connection = connection;
            }

            public async Task InitializeAsync()
            {
                if (_connection.State != ConnectionState.Open)
                {
                    await _connection.OpenAsync();
                }

                // Verificar se a tabela Users existe
                bool tableExists = await CheckIfTableExistsAsync("Users");

                if (!tableExists)
                {
                    // Criar a tabela Users
                    await CreateUsersTableAsync();

                    // Inserir dados de demonstração
                    await InsertDemoDataAsync();
                }
            }

            private async Task<bool> CheckIfTableExistsAsync(string tableName)
            {
                string sql = @"
                SELECT COUNT(*)
                FROM INFORMATION_SCHEMA.TABLES
                WHERE TABLE_NAME = @TableName";

                int result = 0;
                        
                using (var cmd = new SqlCommand(sql, _connection))
                {
                    cmd.Parameters.AddWithValue("@TableName", tableName);

                     var r = await cmd.ExecuteScalarAsync();
                    if (r != null && r != DBNull.Value) {
                        result = Convert.ToInt32(r);
                    }
                }

                return Convert.ToInt32(result) > 0;
            }

            private async Task CreateUsersTableAsync()
            {
                string sql = @"
                CREATE TABLE Users (
                    Id UNIQUEIDENTIFIER PRIMARY KEY,
                    Name NVARCHAR(100) NOT NULL,
                    Email NVARCHAR(100) NOT NULL,
                    CreatedAt DATETIME2 NOT NULL
                )";

                using (var cmd = new SqlCommand(sql, _connection))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            private async Task InsertDemoDataAsync()
            {
                // Inserir dois usuários de demonstração
                var users = new List<User>
            {
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Jon Doe",
                    Email = "jon.doe@example.com",
                    CreatedAt = DateTime.UtcNow
                },
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Isaac AI",
                    Email = "isaac.ai@example.com",
                    CreatedAt = DateTime.UtcNow
                }
            };

                foreach (var user in users)
                {
                    string sql = @"
                    INSERT INTO Users (Id, Name, Email, CreatedAt)
                    VALUES (@Id, @Name, @Email, @CreatedAt)";

                    using (var cmd = new SqlCommand(sql, _connection))
                    {
                        cmd.Parameters.AddWithValue("@Id", user.Id);
                        cmd.Parameters.AddWithValue("@Name", user.Name);
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        cmd.Parameters.AddWithValue("@CreatedAt", user.CreatedAt);

                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }
    }
}
