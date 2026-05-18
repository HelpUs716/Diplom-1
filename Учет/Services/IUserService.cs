using System.Windows;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Учет.Models;
using UserEntity = Учет.Models.User;

namespace Учет.Services
{
    public interface IUserService
    {
        Task<UserEntity?> AuthenticateAsync(string login, string password);
    }
}