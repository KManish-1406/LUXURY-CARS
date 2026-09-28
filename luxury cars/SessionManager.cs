using System.Collections.Generic;

namespace luxury_cars
{
    public static class SessionManager
    {
        public static int UserId { get; set; }
        public static string Username { get; set; } = "";
        public static int RoleId { get; set; }
        public static string RoleName { get; set; } = "";
        public static List<string> Permissions { get; set; } = new();

        public static bool HasPermission(string permission)
        {
            return Permissions.Contains(permission);
        }

        public static void Clear()
        {
            UserId = 0;
            Username = "";
            RoleId = 0;
            RoleName = "";
            Permissions = new();
        }
    }
}