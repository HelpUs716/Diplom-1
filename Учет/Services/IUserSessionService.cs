using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Учет.Enums;

namespace Учет.Services
{
    public interface IUserSessionService
    {
        UserRole CurrentRole { get; }
        string CurrentLogin { get; }
        bool IsAuthenticated { get; }
        void SetSession(UserRole role, string login);
        void ClearSession();
    }
}