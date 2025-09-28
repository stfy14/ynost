// Создайте новый файл, например, Services/AuthService.cs
using Ynost.ViewModels; // для LoginResultRole

namespace Ynost.Services
{
    public static class AuthService
    {
        public static LoginResultRole CurrentUserRole { get; set; } = LoginResultRole.None;

        public static bool CanEdit => CurrentUserRole == LoginResultRole.Editor;
    }
}