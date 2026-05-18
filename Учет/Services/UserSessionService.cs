using Учет.Enums;

namespace Учет.Services
{
    public class UserSessionService : IUserSessionService
    {
        public UserRole CurrentRole { get; private set; }
        public string CurrentLogin { get; private set; } = string.Empty;
        public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentLogin);

        public void SetSession(UserRole role, string login)
        {
            CurrentRole = role;
            CurrentLogin = login;
        }

        public void ClearSession()
        {
            CurrentRole = UserRole.Operator;
            CurrentLogin = string.Empty;
        }
    }
}