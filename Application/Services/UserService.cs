using Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Application.Services
{
    public class UserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Domain.Entities.User> GetByIdAsync(Guid id)
        {
            return await _userRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Domain.Entities.User>> GetAllAsync()
        {
            return await _userRepository.GetAllAsync();
        }

        public async Task<Domain.Entities.User> GetByEmailAsync(string email)
        {
            return await _userRepository.GetByEmailAsync(email);
        }

        public async Task CreateUserAsync(Domain.Entities.User user)
        {
            // Garantir que o ID seja um novo GUID se não for fornecido
            if (user.Id == Guid.Empty)
            {
                user.Id = Guid.NewGuid();
            }

            // Definir a data de criação
            user.CreatedAt = DateTime.UtcNow;

            await _userRepository.AddAsync(user);
        }

        public async Task UpdateUserAsync(Domain.Entities.User user)
        {
            await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(Guid id)
        {
            await _userRepository.DeleteAsync(id);
        }
    }
}
