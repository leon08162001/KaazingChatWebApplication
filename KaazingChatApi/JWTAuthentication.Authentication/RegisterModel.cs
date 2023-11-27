using System.ComponentModel.DataAnnotations;

namespace KaazingChatApi.JWTAuthentication.Authentication
{
    /// <summary>
    /// 註冊時使用的類別
    /// </summary>
    public class RegisterModel
    {
        /// <summary>
        /// 使用者名稱
        /// </summary>
        [Required(ErrorMessage = "User Name is required")]
        public string? Username { get; set; }

        /// <summary>
        /// 使用者Email
        /// </summary>
        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }

        /// <summary>
        /// 使用者密碼
        /// </summary>
        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }

    }
}
