using Domain.Entities;
using Domain.Repositories;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class SqlUserRepository : IUserRepository
    {
        private readonly SqlConnection _connection;

        public SqlUserRepository(SqlConnection connection)
        {
            _connection = connection;
        }

        public async Task<User> GetByIdAsync(Guid id)
        {
            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync();

            const string sql = "SELECT Id, Name, Email, CreatedAt FROM Users WHERE Id = @Id";

            using (var cmd = new SqlCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@Id", id);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new User
                        {
                            Id = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            Email = reader.GetString(2),
                            CreatedAt = reader.GetDateTime(3)
                        };
                    }
                }
            }

            return null;
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync();

            const string sql = "SELECT Id, Name, Email, CreatedAt FROM Users WHERE Email = @Email";

            using (var cmd = new SqlCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@Email", email);

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        return new User
                        {
                            Id = reader.GetGuid(0),
                            Name = reader.GetString(1),
                            Email = reader.GetString(2),
                            CreatedAt = reader.GetDateTime(3)
                        };
                    }
                }
            }
            return null;
        }

        public async Task<IEnumerable<User>> GetAllAsync()
        {
            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync();

            var users = new List<User>();
            const string sql = "SELECT Id, Name, Email, CreatedAt FROM Users";

            using (var cmd = new SqlCommand(sql, _connection))
            using (var reader = await cmd.ExecuteReaderAsync())
            {

                while (await reader.ReadAsync())
                {
                    users.Add(new User
                    {
                        Id = reader.GetGuid(0),
                        Name = reader.GetString(1),
                        Email = reader.GetString(2),
                        CreatedAt = reader.GetDateTime(3)
                    });
                }
            }

            return users;
        }

        public async Task AddAsync(User user)
        {
            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync();

            const string sql = @"
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

        public async Task UpdateAsync(User user)
        {
            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync();

            const string sql = @"
                UPDATE Users
                SET Name = @Name, Email = @Email
                WHERE Id = @Id";

            using (var cmd = new SqlCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@Id", user.Id);
                cmd.Parameters.AddWithValue("@Name", user.Name);
                cmd.Parameters.AddWithValue("@Email", user.Email);

                await cmd.ExecuteNonQueryAsync();
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            if (_connection.State != ConnectionState.Open)
                await _connection.OpenAsync();

            const string sql = "DELETE FROM Users WHERE Id = @Id";

            using (var cmd = new SqlCommand(sql, _connection))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
