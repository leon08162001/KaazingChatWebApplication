namespace KaazingChatApi.JWTAuthentication.Authentication
{
    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string User = "User";
    }
    /// <summary>
    /// 角色
    /// </summary>
    public enum Role
    {
        /// <summary>
        /// 管理者
        /// </summary>
        Admin,
        /// <summary>
        /// 一般使用者
        /// </summary>
        User
    }
}
