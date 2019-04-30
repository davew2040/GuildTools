using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GuildTools.Configuration;
using GuildTools.Controllers.Models;
using GuildTools.Data;
using GuildTools.EF.Models.Enums;
using GuildTools.Models;
using GuildTools.Permissions;
using GuildTools.Services;
using GuildTools.Services.Mail;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GuildTools.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly JwtSettings jwtSettings;
        private readonly ConnectionStrings connectionStrings;
        private readonly ICommonValuesProvider commonValues;
        private readonly Sql.Accounts accountsSql;
        private readonly IMailSender mailSender;
        private readonly IDataRepository dataRepository;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IMailSender mailSender,
            IDataRepository dataRepository,
            ICommonValuesProvider commonValues,
            IOptions<JwtSettings> jwtSettings,
            IOptions<ConnectionStrings> connectionStrings)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.mailSender = mailSender;
            this.dataRepository = dataRepository;
            this.commonValues = commonValues;
            this.jwtSettings = jwtSettings.Value;
            this.connectionStrings = connectionStrings.Value;
            this.accountsSql = new Sql.Accounts(this.connectionStrings.Database);
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationCredentials credentials)
        {
            if (!ModelState.IsValid)
            {
                return Error("Encountered invalid user values during registration.");
            }

            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = credentials.Email, Email = credentials.Email };

                var result = await this.userManager.CreateAsync(user, credentials.Password);

                if (result.Succeeded)
                {
                    await this.userManager.AddToRoleAsync(user, GuildToolsRoles.StandardUser.Name);

                    await userManager.AddClaimAsync(user, new Claim(GuildToolsClaims.UserId, user.Id));

                    this.accountsSql.UpdateUsername(user.Id, credentials.Username, connectionStrings.Database);
                    await this.signInManager.SignInAsync(user, isPersistent: false);

                    var authenticationResponse = await this.GetAuthenticationResponse(user);

                    return new JsonResult(authenticationResponse);
                }
                else
                {
                    if (result.Errors.Any(e => e.Code == "DuplicateUserName"))
                    {
                        return Error("This email address is already registered.");
                    }
                }

                return Errors(result);

            }
            return Error("Unexpected error");
        }

        [HttpGet]
        [Route("resetPassword")]
        public async Task<IActionResult> ResetPassword(string emailAddress)
        {
            var user = await this.userManager.FindByEmailAsync(emailAddress);

            if (user == null)
            {
                return Error("No user registered with this email address.");
            }

            string resetToken = await this.userManager.GeneratePasswordResetTokenAsync(user);

            string baseUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";
            string resetUrl = $"{baseUrl}/resetpasswordtoken?userId={user.Id}&token={resetToken}";

            var resetEmail = MailGenerator.GenerateResetPasswordEmail(resetUrl);
            var result = await this.mailSender.SendMailAsync(user.Email, this.commonValues.AdminEmail, resetEmail.Subject, resetEmail.TextContent, resetEmail.HtmlContent);

            if (result)
            {
                return Ok();
            }
            else
            {
                return Error("Failed to send reset password email.");
            }
        }

        public class ResetPasswordWithTokenModel
        {
            public string UserId { get; set; }
            public string Token { get; set; }
            public string NewPassword { get; set; }
        }

        [HttpPost]
        [Route("resetPasswordToken")]
        public async Task<IActionResult> ResetPasswordWithToken([FromBody] ResetPasswordWithTokenModel model)
        {
            var user = await this.userManager.FindByIdAsync(model.UserId);

            if (user == null)
            {
                return Error($"No user with id {model.UserId}.");
            }

            var resetResult = await this.userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);

            if (resetResult.Succeeded)
            {
                return Ok();
            }
            else
            {
                return Errors(resetResult);
            }
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginCredentials credentials)
        {
            if (!ModelState.IsValid)
            {
                return Error("Encountered invalid user values during registration.");
            }

            var result = await signInManager.PasswordSignInAsync(credentials.Email, credentials.Password, false, false);

            if (result.Succeeded)
            {
                var user = await userManager.FindByEmailAsync(credentials.Email);
                
                var authenticationResponse = await this.GetAuthenticationResponse(user);

                return new JsonResult(authenticationResponse);
            }

            return new JsonResult("Unable to sign in.") { StatusCode = 401 };
        }

        private async Task<Dictionary<string, object>> GetAuthenticationResponse(IdentityUser user)
        {
            return new Dictionary<string, object>
            {
                { "access_token", this.GetJwt(user.Id, user.Email) },
                { "email", user.Email }
            };
        }

        private string GetJwt(string userId, string email)
        {
            string serializedToken;

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(this.jwtSettings.SecretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim(GuildToolsClaims.UserId, userId)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            serializedToken = tokenHandler.WriteToken(token);

            return serializedToken;
        }

        private JsonResult Errors(IdentityResult result)
        {
            var items = result.Errors
                .Select(x => x.Description)
                .ToArray();
            return new JsonResult(items) { StatusCode = 400 };
        }

        private JsonResult Error(string message)
        {
            return new JsonResult(message) { StatusCode = 400 };
        }

        private static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }
    }
}