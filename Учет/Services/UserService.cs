using System.Windows;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Linq;
using System.Threading.Tasks;
using Учет.Data.Interfaces;
using Учет.Models;
using UserEntity = Учет.Models.User;

namespace Учет.Services
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _uow;
        public UserService(IUnitOfWork uow) => _uow = uow;

        public async Task<UserEntity?> AuthenticateAsync(string login, string password)
        {
            var users = await _uow.Users.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Login.Equals(login, StringComparison.OrdinalIgnoreCase));
            if (user == null) return null;

            if (user.PasswordHash == password)
                return user;

            return null;
        }
    }
}