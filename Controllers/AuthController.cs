using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;
using MyStoreAPI.Areas.Identity.Data;
using MyStoreAPI.Models;
using MyStoreAPI.Services;
using MyStoreAPI.Static;

namespace MyStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly UserManager<MyStoreAPIUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly EmailService _emailService;
        public AuthController(ILogger<AuthController> logger, UserManager<MyStoreAPIUser> userManager, RoleManager<IdentityRole> roleManager,
            IConfiguration configuration, IServiceProvider serviceProvider, EmailService emailService)
        {
            _logger = logger;
            this.userManager = userManager;
            this.roleManager = roleManager;
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _emailService = emailService;
        }

        [Authorize(Policy = "AdminPolicy")]
        [HttpPost]
        [Route("register-admin")]
        public async Task<IActionResult> RegisterAdmin(RegisterModel model)
        {
            try
            {
                var userExists = await userManager.FindByNameAsync(model.Username);
                if (userExists != null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel<string> { Status = "Error", Message = "Admin User already exists!" });

                MyStoreAPIUser user = new()
                {
                    Email = model.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.Username,
                    FirstName = model.FirstName,
                    LastName = model.LastName
                };
                var result = await userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                    return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel<string> { Status = "Error", Message = "User creation failed! Please check user details and try again." });

                if (!await roleManager.RoleExistsAsync(UserRole.Admin))
                    await roleManager.CreateAsync(new IdentityRole(UserRole.Admin));

                if (await roleManager.RoleExistsAsync(UserRole.Admin))
                {
                    await userManager.AddToRoleAsync(user, UserRole.Admin);
                }

                return Ok(new ResponseModel<string> { Status = "Success", Message = "Admin User created successfully!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            try
            {
                var userExists = await userManager.FindByNameAsync(model.Username);
                if (userExists != null)
                    return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel<string> { Status = "Error", Message = "User already exists!" });

                MyStoreAPIUser user = new()
                {
                    Email = model.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.Username,
                    FirstName = model.FirstName,
                    LastName = model.LastName

                };
                var result = await userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                    return StatusCode(StatusCodes.Status500InternalServerError, new ResponseModel<string> { Status = "Error", Message = "User creation failed! Please check user details and try again." });

                var roleManager = _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                if (!await roleManager.RoleExistsAsync("User"))
                {
                    await roleManager.CreateAsync(new IdentityRole("User"));
                }

                await userManager.AddToRoleAsync(user, UserRole.User);

                return Ok(new ResponseModel<string> { Status = "Success", Message = "User created successfully!", StatusCode = 200 });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login(LoginModel model)
        {
            try
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    return NotFound(new ResponseModel<string> { Status = "Error", StatusCode = 404, Message = "The email you entered does not match any account. Please check your email and try again." });
                }

                // Check the password
                if (!await userManager.CheckPasswordAsync(user, model.Password))
                {
                    // Invalid password
                    return Unauthorized(new ResponseModel<string> { Status = "Error", StatusCode = 401, Message = "If you have forgotten your password, you can reset it using the 'Forgot password?' link below." });
                }

                var userRoles = await userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString())
                    };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                string token = GenerateToken(authClaims);

                return Ok(new ResponseModel<string> { Status = "Success", StatusCode = 200, Message = "You have successfully logged in and can now access your account.", Data = token, Items = (List<string>)userRoles });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [Route("{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            try
            {
                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    // Return a proper ResponseModel with an error message
                    return Ok(new ResponseModel<string>
                    {
                        Status = "Warning",
                        StatusCode = 404,
                        Message = $"User not found.",
                        Data = null
                    });
                }

                return Ok(new ResponseModel<UserDetailApiModel>
                {
                    Status = "Success",
                    StatusCode = 200,
                    Message = "User Details Successful",
                    Data = new UserDetailApiModel
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [Route("forgot-password")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel forgotPassModel)
        {
            var user = await userManager.FindByEmailAsync(forgotPassModel.Email);
            if (user == null)
            {
                // Return a proper ResponseModel with an error message
                return Ok(new ResponseModel<string>
                {
                    Status = "Warning",
                    StatusCode = 404,
                    Message = $"Email address not found {forgotPassModel.Email}0.",
                    Data = null
                });
            }

            // Generate a JWT token for password reset
            var authClaims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim("Purpose", "PasswordReset"), // Custom claim to ensure token is for reset
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique token ID
            };

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(authClaims),
                Expires = DateTime.UtcNow.AddHours(1), // Token valid for 1 hour
                Issuer = _configuration["Jwt:ValidIssuer"],
                Audience = _configuration["Jwt:ValidAudience"],
                SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var resetToken = tokenHandler.WriteToken(token);


            //var resetLink = Url.Action("ResetPassword", "Auth", new { email = forgotPassModel.Email, token }, Request.Scheme);
            var resetLink = $"http://localhost:5008/reset-password?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(resetToken)}";

            // Send reset email
            var emailBody = $@"
                <h3>Password Reset Request</h3>
                <p>Click the link below to reset your password:</p>
                <a href='{resetLink}'>Reset Password</a>
            ";
            await _emailService.SendEmailAsync(forgotPassModel.Email, "Password Reset Request", emailBody);

            return Ok(new ResponseModel<ForgotPasswordModel>
            {
                Status = "Success",
                StatusCode = 200,
                Message = "Password reset email sent",
                Data = new ForgotPasswordModel
                {
                    Email = user.Email
                }
            });
        }


        [HttpPost]
        [Route("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery] string email, [FromQuery] string token, [FromBody] ResetPasswordModel resetPassModel)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return BadRequest(new { Status = "Error", Message = "Invalid request parameters" });
            }

            var handler = new JwtSecurityTokenHandler();
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:ValidIssuer"],
                ValidAudience = _configuration["Jwt:ValidAudience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]))
            };            

            try
            {
                var principal = handler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                var emailClaim = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
                var purposeClaim = principal.FindFirst("Purpose")?.Value;

                if (purposeClaim != "PasswordReset")
                {
                    return BadRequest(new { Status = "Error", Message = "Invalid token purpose" });
                }

                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return NotFound(new { Status = "Error", Message = "User not found" });
                }

                // Manually reset the password
                var hashedPassword = userManager.PasswordHasher.HashPassword(user, resetPassModel.NewPassword);
                user.PasswordHash = hashedPassword;

                // Update the user in the database
                var updateResult = await userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    return BadRequest(new
                    {
                        Status = "Error",
                        Message = "Password reset failed",
                        Errors = updateResult.Errors.Select(e => e.Description)
                    });
                }

                return Ok(new ResponseModel<ResetPasswordModel>
                {
                    Status = "Success",
                    StatusCode = 200,
                    Message = "Password has been reset successfully.",
                    Data = new ResetPasswordModel
                    {
                        Email = resetPassModel.Email,
                        Token = resetPassModel.Token,
                        NewPassword = resetPassModel.NewPassword
                    }
                });
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { Status = "Error", Message = "Invalid or expired token", Details = ex.Message });
            }
            catch (Exception ex)
            {
                // Log exception for debugging
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "An internal server error occurred.",
                    Details = ex.Message
                });
            }
        }


        private string GenerateToken(List<Claim> authClaims)
        {
            var authSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(_configuration["JWT:Secret"]));
            var TokenExpiryTimeInHour = Convert.ToInt64(_configuration["JWT:TokenExpiryTimeInHour"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["JWT:ValidIssuer"],
                Audience = _configuration["JWT:ValidAudience"],
                Expires = DateTime.UtcNow.AddHours(TokenExpiryTimeInHour),
                SigningCredentials = new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256),
                Subject = new ClaimsIdentity(authClaims),
                
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
