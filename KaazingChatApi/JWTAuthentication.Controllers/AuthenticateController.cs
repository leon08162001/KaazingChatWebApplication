using KaazingChatApi.JWTAuthentication.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace KaazingChatApi.JWTAuthentication.Controllers
{
    /// <summary>
    /// JWT認證授權For Api
    /// </summary>
    public class AuthenticateController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;
        private readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AuthenticateController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
        }

        /// <summary>
        /// 使用登入取得Api授權Token
        /// </summary>
        /// <param name="model">登入時使用的類別</param>
        /// <response code="200">登入成功，傳回Api授權Token(型別:{token=xxx,expiration=xxx})。</response>
        /// <response code="401">登入不成功，傳回未授權。</response> 
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                var user = await userManager.FindByNameAsync(model.Username);
                if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
                {
                    var userRoles = await userManager.GetRolesAsync(user);

                    var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

                    foreach (var userRole in userRoles)
                    {
                        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                    }

                    var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

                    var token = new JwtSecurityToken(
                        issuer: _configuration["JWT:ValidIssuer"],
                        audience: _configuration["JWT:ValidAudience"],
                        expires: DateTime.Now.AddHours(3),
                        claims: authClaims,
                        signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                        );

                    return Ok(new
                    {
                        token = new JwtSecurityTokenHandler().WriteToken(token),
                        expiration = token.ValidTo
                    });
                }
                return Unauthorized();
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 註冊指定使用者為管理員角色
        /// </summary>
        /// <param name="model">註冊時使用的類別</param>
        /// <response code="200">註冊指定使用者為管理員角色成功，回應Status = \"Success\", Message = \"XXX\"。</response>
        /// <response code="500">註冊指定使用者為管理員角色不成功，回應Status = \"Error\", Message = \"XXX\"。</response> 
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin(RegisterModel model)
        {
            try
            {
                var userExists = await userManager.FindByNameAsync(model.Username);
                if (userExists != null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"User({model.Username}) already exists!" });

                var userExistsByEmail = await userManager.FindByEmailAsync(model.Email);
                if (userExistsByEmail != null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"Email({model.Email}) already exists!" });

                ApplicationUser user = new ApplicationUser()
                {
                    Email = model.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.Username
                };
                var result = await userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"User({model.Username}) creation failed! Please check user details and try again." });

                if (!await roleManager.RoleExistsAsync(UserRoles.Admin))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));
                if (!await roleManager.RoleExistsAsync(UserRoles.User))
                    await roleManager.CreateAsync(new IdentityRole(UserRoles.User));

                if (await roleManager.RoleExistsAsync(UserRoles.Admin))
                {
                    await userManager.AddToRoleAsync(user, UserRoles.Admin);
                }

                return Ok(new Response { Status = "Success", Message = $"User({model.Username}) created successfully!" });
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 註冊指定使用者(不包括任何角色)
        /// </summary>
        /// <param name="model">註冊時使用的類別</param>
        /// <response code="200">註冊指定使用者(不包括任何角色)成功，回應Status = \"Success\", Message = \"XXX\"。</response>
        /// <response code="500">註冊指定使用者(不包括任何角色)不成功，回應Status = \"Error\", Message = \"XXX\"。</response> 
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            try
            {
                var userExists = await userManager.FindByNameAsync(model.Username);
                if (userExists != null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"User({model.Username}) already exists!" });

                var userExistsByEmail = await userManager.FindByEmailAsync(model.Email);
                if (userExistsByEmail != null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"Email({model.Email}) already exists!" });

                ApplicationUser user = new ApplicationUser()
                {
                    Email = model.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.Username
                };
                var result = await userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"User({model.Username}) creation failed! Please check user details and try again." });

                return Ok(new Response { Status = "Success", Message = $"User({model.Username}) created successfully!" });
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 取消註冊(刪除)指定使用者(該使用者所包含的角色一併刪除)
        /// </summary>
        /// <param name="model">取消註冊時使用的類別</param>
        /// <response code="200">取消註冊(刪除)指定使用者(該使用者所包含的角色一併刪除)成功，回應Status = \"Success\", Message = \"XXX\"。</response>
        /// <response code="500">取消註冊(刪除)指定使用者(該使用者所包含的角色一併刪除)不成功，回應Status = \"Error\", Message = \"XXX\"。</response> 
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("unRegister")]
        public async Task<IActionResult> UnRegister(UnRegisterModel model)
        {
            try
            {
                IdentityOptions identityOptions = new IdentityOptions();
                var userExistsByUsername = await userManager.FindByNameAsync(model.Username);
                var userExistsByEmail = await userManager.FindByEmailAsync(model.Email);

                //使用使用者名稱及Email找到的用戶資訊並非都存在
                if (!(userExistsByUsername != null && userExistsByEmail != null))
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"User({model.Username}) not exists in [AspNetUsers]!" });
                //若兩者用戶資訊都存在,但並非兩者的UserName和Email欄位值都相同
                if (!(userExistsByUsername.UserName.Equals(userExistsByEmail.UserName) && userExistsByUsername.Email.Equals(userExistsByEmail.Email)))
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"User({model.Username})'s data is inconsistent in [AspNetUsers]!" });

                var result = await userManager.DeleteAsync(userExistsByUsername);
                if (!result.Succeeded)
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"User({model.Username}) deletion failed! Please check user details and try again." });

                var userRoles = Enum.GetNames(typeof(Role)).ToList();

                foreach (object? role in userRoles)
                {
                    if (await roleManager.RoleExistsAsync(role.ToString()))
                    {
                        await userManager.RemoveFromRoleAsync(userExistsByUsername, role.ToString());
                    }
                }

                return Ok(new Response { Status = "Success", Message = $"User({model.Username}) deleted successfully!" });
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 將指定使用者加入指定角色
        /// </summary>
        /// <param name="model">指定的使用者</param>
        /// <param name="role">指定的角色</param>
        /// <response code="200">將指定使用者加入指定角色成功，回應Status = \"Success\", Message = \"XXX\"。</response>
        /// <response code="500">將指定使用者加入指定角色不成功，回應Status = \"Error\", Message = \"XXX\"。</response> 
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("addUserRole")]
        public async Task<IActionResult> AddUserRole(UserModel model, [FromQuery, BindRequired] Role role)
        {
            try
            {
                var user = await userManager.FindByNameAsync(model.Username);
                if (user == null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"User({model.Username}) not exists!" });

                if (await roleManager.RoleExistsAsync(Enum.GetName(typeof(Role), role)))
                {
                    var userRoles = await userManager.GetRolesAsync(user);
                    if (!userRoles.Contains(Enum.GetName(typeof(Role), role)))
                    {
                        var result = await userManager.AddToRoleAsync(user, Enum.GetName(typeof(Role), role));
                        if (result.Errors.Count() == 0)
                        {
                            return Ok(new Response { Status = "Success", Message = $"User({model.Username}) has added a role({Enum.GetName(typeof(Role), role)}) successfully!" });
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"User({model.Username}) has added a role({Enum.GetName(typeof(Role), role)}) unsuccessfully!" });
                        }
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"Role({Enum.GetName(typeof(Role), role)}) already exists in [AspNetUserRoles]!" });
                    }
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"Role({Enum.GetName(typeof(Role), role)}) not exists in [AspNetRoles]!" });
                }
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 將指定使用者加入多個指定角色
        /// </summary>
        /// <param name="model">指定的使用者</param>
        /// <param name="roles">多個指定的角色</param>
        /// <response code="200">將指定使用者加入多個指定角色成功，回應Status = \"Success\", Message = \"XXX\"。</response>
        /// <response code="500">將指定使用者加入多個指定角色不成功，回應Status = \"Error\", Message = \"XXX\"。</response> 
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("addUserRoles")]
        public async Task<IActionResult> AddUserRoles(UserModel model, [FromBody, BindRequired] List<Role> roles)
        {
            try
            {
                var user = await userManager.FindByNameAsync(model.Username);
                if (user == null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"User({model.Username}) not exists!" });

                var errorMessage = "";
                var sAllRolesForAdd = "";
                List<string> rolesListForAdd = new List<string>();
                foreach (var role in roles)
                {
                    if (await roleManager.RoleExistsAsync(Enum.GetName(typeof(Role), role)))
                    {
                        var userRoles = await userManager.GetRolesAsync(user);
                        if (!userRoles.Contains(Enum.GetName(typeof(Role), role)))
                        {
                            //var result = await userManager.AddToRoleAsync(user, Enum.GetName(typeof(Role), role));
                            //if (result.Errors.Count() == 0)
                            //{
                            //    sAllRolesInSuccess += Enum.GetName(typeof(Role), role) + "、";
                            //}
                            //else
                            //{
                            //    errorMessage += $"User({model.Username}) has added a role({Enum.GetName(typeof(Role), role)}) unsuccessfully!" + Environment.NewLine;
                            //}
                            rolesListForAdd.Add(Enum.GetName(typeof(Role), role));
                            sAllRolesForAdd += Enum.GetName(typeof(Role), role) + "、";
                        }
                        else
                        {
                            errorMessage += $"Role({Enum.GetName(typeof(Role), role)}) already exists in [AspNetUserRoles]!" + Environment.NewLine;
                        }
                    }
                    else
                    {
                        errorMessage += $"Role({Enum.GetName(typeof(Role), role)}) not exists in [AspNetRoles]!" + Environment.NewLine;
                    }
                }

                if (errorMessage.Equals(""))
                {
                    sAllRolesForAdd = sAllRolesForAdd.Remove(sAllRolesForAdd.Length - 1, 1);
                    var result = await userManager.AddToRolesAsync(user, rolesListForAdd);
                    if (result.Errors.Count() == 0)
                        return Ok(new Response { Status = "Success", Message = $"User({model.Username}) has added roles({sAllRolesForAdd}) successfully!" });
                    else
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"User({model.Username}) has added roles({sAllRolesForAdd}) unsuccessfully!" });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 將指定使用者移除指定角色
        /// </summary>
        /// <param name="model">指定的使用者</param>
        /// <param name="role">指定的角色</param>
        /// <response code="200">將將指定使用者移除指定角色成功，回應Status = \"Success\", Message = \"XXX\"。</response>
        /// <response code="500">將指定使用者移除指定角色不成功，回應Status = \"Error\", Message = \"XXX\"。</response> 
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("removeUserRole")]
        public async Task<IActionResult> RemoveUserRole(UserModel model, [FromQuery, BindRequired] Role role)
        {
            try
            {
                var user = await userManager.FindByNameAsync(model.Username);
                if (user == null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"User({model.Username}) not exists!" });

                if (await roleManager.RoleExistsAsync(Enum.GetName(typeof(Role), role)))
                {
                    var userRoles = await userManager.GetRolesAsync(user);
                    if (userRoles.Contains(Enum.GetName(typeof(Role), role)))
                    {
                        var result = await userManager.RemoveFromRoleAsync(user, Enum.GetName(typeof(Role), role));
                        if (result.Errors.Count() == 0)
                        {
                            return Ok(new Response { Status = "Success", Message = $"User({model.Username}) has removed a role({Enum.GetName(typeof(Role), role)}) successfully!" });
                        }
                        else
                        {
                            return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"User({model.Username}) has removed a role({Enum.GetName(typeof(Role), role)}) unsuccessfully!" });
                        }
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"User({model.Username}) didn't had a role({Enum.GetName(typeof(Role), role)}) to be removed!" });
                    }
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"Role({Enum.GetName(typeof(Role), role)}) not exists in [AspNetRoles]!" });
                }
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// 將指定使用者移除多個指定角色
        /// </summary>
        /// <param name="model">指定的使用者</param>
        /// <param name="roles">多個指定的角色</param>
        /// <response code="200">將指定使用者移除多個指定角色成功，回應Status = \"Success\", Message = \"XXX\"。</response>
        /// <response code="500">將指定使用者移除多個指定角色不成功，回應Status = \"Error\", Message = \"XXX\"。</response> 
        /// <response code="400">執行中發生例外錯誤，回應錯誤訊息。</response> 
        [HttpPost]
        //[Route("removeUserRoles")]
        public async Task<IActionResult> RemoveUserRoles(UserModel model, [FromBody, BindRequired] List<Role> roles)
        {
            try
            {
                var user = await userManager.FindByNameAsync(model.Username);
                if (user == null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"User({model.Username}) not exists!" });

                var errorMessage = "";
                var sAllRolesForRemove = "";
                List<string> rolesListForRemove = new List<string>();
                foreach (var role in roles)
                {
                    if (await roleManager.RoleExistsAsync(Enum.GetName(typeof(Role), role)))
                    {
                        var userRoles = await userManager.GetRolesAsync(user);
                        if (userRoles.Contains(Enum.GetName(typeof(Role), role)))
                        {
                            rolesListForRemove.Add(Enum.GetName(typeof(Role), role));
                            sAllRolesForRemove += Enum.GetName(typeof(Role), role) + "、";
                        }
                        else
                        {
                            errorMessage += $"Role({Enum.GetName(typeof(Role), role)}) not exists in [AspNetUserRoles]!" + Environment.NewLine;
                        }
                    }
                    else
                    {
                        errorMessage += $"Role({Enum.GetName(typeof(Role), role)}) not exists in [AspNetRoles]!" + Environment.NewLine;
                    }
                }

                if (errorMessage.Equals(""))
                {
                    sAllRolesForRemove = sAllRolesForRemove.Remove(sAllRolesForRemove.Length - 1, 1);
                    var result = await userManager.RemoveFromRolesAsync(user, rolesListForRemove);
                    if (result.Errors.Count() == 0)
                        return Ok(new Response { Status = "Success", Message = $"User({model.Username}) has removed roles({sAllRolesForRemove}) successfully!" });
                    else
                        return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = $"User({model.Username}) has removed roles({sAllRolesForRemove}) unsuccessfully!" });
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = errorMessage });
                }
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) log.Error(ex.Message, ex);
                return BadRequest(ex.Message);
            }
        }
    }
}
