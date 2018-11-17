using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuildTools.Configuration;
using GuildTools.Models;
using JWT;
using JWT.Algorithms;
using JWT.Serializers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace GuildTools.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly JwtSettings jwtSettings;
        private readonly ConnectionStrings connectionStrings;
        private readonly Sql.Accounts accountsSql;

        public AccountController(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            IOptions<JwtSettings> jwtSettings,
            IOptions<ConnectionStrings> connectionStrings)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
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
                    this.accountsSql.UpdateUsername(user.Id, credentials.Username, connectionStrings.Database);
                    await this.signInManager.SignInAsync(user, isPersistent: false);
                    
                    return new JsonResult(new Dictionary<string, object>
                      {
                        { "access_token", this.GetAccessToken(user.Id, user.Email) },
                        { "id_token", this.GetIdToken(user) }
                      });
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

                return new JsonResult(new Dictionary<string, object>
                {
                    { "access_token", this.GetAccessToken(user.Id, user.Email) },
                    { "id_token", this.GetIdToken(user) }
                });
            }

            return new JsonResult("Unable to sign in.") { StatusCode = 401 };
        }

        private string GetIdToken(IdentityUser user)
        {
            var payload = new Dictionary<string, object>
             {
                { "id", user.Id },
                { "sub", user.UserName },
                { "email", user.Email },
                { "emailConfirmed", user.EmailConfirmed },
             };

            return GetToken(payload);
        }

        private string GetAccessToken(string username, string email)
        {
            var payload = new Dictionary<string, object>
            {
                { "sub", email },
                { "email", email }
            };

            return GetToken(payload);
        }

        private string GetToken(Dictionary<string, object> payload)
        {
            var secret = this.jwtSettings.SecretKey;

            payload.Add("iss", this.jwtSettings.Issuer);
            payload.Add("aud", this.jwtSettings.Audience);
            payload.Add("nbf", ConvertToUnixTimestamp(DateTime.Now));
            payload.Add("iat", ConvertToUnixTimestamp(DateTime.Now));
            payload.Add("exp", ConvertToUnixTimestamp(DateTime.Now.AddDays(7)));

            IJwtAlgorithm algorithm = new HMACSHA256Algorithm();
            IJsonSerializer serializer = new JsonNetSerializer();
            IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder encoder = new JwtEncoder(algorithm, serializer, urlEncoder);

            return encoder.Encode(payload, secret);
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