using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using GuildTools.Configuration;
using GuildTools.Controllers.Models;
using GuildTools.Data;
using GuildTools.EF.Models;
using GuildTools.EF.Models.Enums;
using GuildTools.ErrorHandling;
using GuildTools.Models;
using GuildTools.Permissions;
using GuildTools.Services;
using GuildTools.Services.Mail;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GuildTools.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : GuildToolsController
    {
        private readonly UserManager<UserWithData> userManager;
        private readonly SignInManager<UserWithData> signInManager;
        private readonly JwtSettings jwtSettings;
        private readonly ICommonValuesProvider commonValues;
        private readonly IMailSender mailSender;

        public AccountController(
            UserManager<UserWithData> userManager,
            SignInManager<UserWithData> signInManager,
            IMailSender mailSender,
            ICommonValuesProvider commonValues,
            IOptions<JwtSettings> jwtSettings)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.mailSender = mailSender;
            this.commonValues = commonValues;
            this.jwtSettings = jwtSettings.Value;
        }

        [HttpPost]
        [Route("register")]
        public async Task Register([FromBody] RegistrationDetails details)
        {
            if (!ModelState.IsValid)
            {
                var errorDescriptions = new List<string>();

                foreach (var modelState in ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        errorDescriptions.Add(error.ErrorMessage);
                    }
                }

                throw new UserReportableError("Encountered errors in registration request: "
                    + string.Join(", ", errorDescriptions), (int)HttpStatusCode.BadRequest);
            }

            var user = new UserWithData()
            {
                UserName = details.Username,
                Email = details.Email,
                GuildName = details.GuildName,
                GuildRealm = details.GuildRealm,
                PlayerRegion = details.PlayerRegion,
                PlayerName = details.PlayerName,
                PlayerRealm = details.PlayerRealm
            };

            var result = await this.userManager.CreateAsync(user, details.Password);

            if (result.Errors.Any())
            {
                var errorsMessages = string.Join(", ", result.Errors.Select(x => x.Description));
                throw new UserReportableError("Encountered errors creating this user account: "
                    + errorsMessages, (int)HttpStatusCode.BadRequest);
            }

            if (result.Succeeded)
            {
                await this.userManager.AddToRoleAsync(user, GuildToolsRoles.StandardUser.Name);

                await userManager.AddClaimAsync(user, new Claim(GuildToolsClaims.UserId, user.Id));

                var confirmationToken = await this.userManager.GenerateEmailConfirmationTokenAsync(user);

                string baseUrl = this.GetBaseUrl();
                string resetUrl = $"{baseUrl}/confirmemail?userId={user.Id}&token={HttpUtility.UrlEncode(confirmationToken)}";

                var confirmationEmail = MailGenerator.GenerateRegistrationConfirmationEmail(resetUrl);

                var mailResult = await this.mailSender.SendMailAsync(
                    user.Email, 
                    this.commonValues.AdminEmail, 
                    confirmationEmail.Subject, 
                    confirmationEmail.TextContent, 
                    confirmationEmail.HtmlContent);

                if (!mailResult)
                {
                    await this.userManager.DeleteAsync(user);
                    throw new UserReportableError("An error occurred while attempting to create this account.", (int)HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                if (result.Errors.Any(e => e.Code == "DuplicateUserName"))
                {
                    throw new UserReportableError("This email address is already registered.", (int)HttpStatusCode.BadRequest);
                }

                throw new UserReportableError("An error occurred while attempting to create this account.", (int)HttpStatusCode.InternalServerError);
            }
        }

        [HttpPost]
        [Route("confirmEmail")]
        public async Task ConfirmEmail([FromBody] ConfirmEmail confirmation)
        {
            if (!ModelState.IsValid)
            {
                throw new UserReportableError("Encountered invalid user values.", (int)HttpStatusCode.BadRequest);
            }

            var user = await this.userManager.FindByIdAsync(confirmation.UserId);
            if (user == null)
            {
                throw new UserReportableError("Could not find a user with this ID.", (int)HttpStatusCode.BadRequest);
            }

            var confirmationResult = await this.userManager.ConfirmEmailAsync(user, confirmation.Token);

            if (!confirmationResult.Succeeded)
            {
                throw new UserReportableError("Unable to confirmation this email address.", (int)HttpStatusCode.BadRequest);
            }
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

            string baseUrl = this.GetBaseUrl();
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
        public async Task<LoginResponse> Login([FromBody] LoginCredentials credentials)
        {
            if (!ModelState.IsValid)
            {
                var errorDescriptions = new List<string>();

                foreach (var modelState in ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        errorDescriptions.Add(error.ErrorMessage);
                    }
                }

                throw new Exception("Encountered errors: " + string.Join(", ", errorDescriptions));
            }

            var user = await this.userManager.FindByEmailAsync(credentials.Email);

            if (user == null)
            {
                throw new UserReportableError($"Couldn't find user with email '{credentials.Email}'.", (int)HttpStatusCode.BadRequest);
            }

            if (!user.EmailConfirmed)
            {
                throw new UserReportableError($"User with email '{credentials.Email}' hasn't yet confirmed their registration.", (int)HttpStatusCode.BadRequest);
            }

            var signinResult = await signInManager.PasswordSignInAsync(user.UserName, credentials.Password, false, false);

            if (signinResult.Succeeded)
            {
                var authenticationResponse = this.GetAuthenticationResponse(user);

                return new LoginResponse()
                {
                    AuthenticationDetails = authenticationResponse,
                    Email = user.Email,
                    Username = user.UserName,
                    GuildName = user.GuildName,
                    GuildRealm = user.GuildRealm,
                    PlayerName = user.PlayerName,
                    PlayerRealm = user.PlayerRealm,
                    PlayerRegion = user.PlayerRegion
                };
            }

            throw new UserReportableError("Unable to sign in.", (int)HttpStatusCode.Unauthorized);
        }

        private Dictionary<string, object> GetAuthenticationResponse(IdentityUser user)
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