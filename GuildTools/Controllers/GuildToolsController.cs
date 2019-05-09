using GuildTools.Cache;
using GuildTools.Cache.SpecificCaches;
using GuildTools.Cache.SpecificCaches.CacheInterfaces;
using GuildTools.Controllers.InputModels;
using GuildTools.Controllers.JsonResponses;
using GuildTools.Controllers.Models;
using GuildTools.Data;
using GuildTools.EF.Models.Enums;
using GuildTools.ErrorHandling;
using GuildTools.ExternalServices.Blizzard;
using GuildTools.Permissions;
using GuildTools.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static GuildTools.EF.Models.Enums.EnumUtilities;
using static GuildTools.ExternalServices.Blizzard.BlizzardService;
using EfModels = GuildTools.EF.Models;
using EfEnums = GuildTools.EF.Models.Enums;
using EfBlizzardModels = GuildTools.EF.Models.StoredBlizzardModels;
using GuildTools.Cache.LongRunningRetrievers;

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
