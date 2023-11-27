using System.ComponentModel.DataAnnotations;

namespace KaazingChatApi.JWTAuthentication.Authentication
{
    /// <summary>
    /// 使用者類別
    /// </summary>
    public class UserModel
    {
        /// <summary>
        /// 使用者名稱
        /// </summary>
        [Required(ErrorMessage = "User Name is required")]
        public string? Username { get; set; }

        /// <summary>
        /// 使用者Email
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 使用者電話
        /// </summary>
        public string? PhoneNumber { get; set; }
    }
}
