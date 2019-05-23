using Microsoft.AspNetCore.Mvc;

namespace GuildTools.Controllers
{
    public abstract class GuildToolsController : Controller
    {
        protected string GetBaseUrl()
        {
            string baseUrl = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

            return baseUrl;
        }
    }
}
