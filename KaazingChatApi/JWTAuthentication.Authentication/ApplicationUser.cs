using Microsoft.AspNetCore.Identity;

namespace KaazingChatApi.JWTAuthentication.Authentication
{
    public class ApplicationUser : IdentityUser
    {
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
    }
}
