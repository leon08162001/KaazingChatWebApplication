using System.ComponentModel.DataAnnotations;

namespace KaazingChatApi.JWTAuthentication.Authentication
{
    public class TokenModel
    {
        /// <summary>
        /// 刷新令牌
        /// </summary>
        [Required(ErrorMessage = "AccessToken is required")]
        public string? AccessToken { get; set; }
        /// <summary>
        /// 訪問令牌
        /// </summary>
        [Required(ErrorMessage = "RefreshToken is required")]
        public string? RefreshToken { get; set; }
    }
}
