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
using GuildTools.Utilities;
using GuildTools.Cache.LongRunningRetrievers.Interfaces;
using AutoMapper;
using GuildTools.ExternalServices.Blizzard.Utilities;
using GuildTools.Controllers.Models.Stats;
using System.Drawing;

namespace GuildTools.Controllers
{
    [Route("api/[controller]")]
    public class SketchController : GuildToolsController
    {
        private readonly IDataRepository _dataRepo;
        private static readonly SketchManager _sketchManager = new SketchManager();

        public SketchController(
            IDataRepository repository)
        {

        }

        [HttpGet]
        [Route("public")]
        public IActionResult Public()
        {
            return Json(new
            {
                Message = "Hello from a public endpoint! You don't need to be authenticated to see this."
            });
        }
    }

    public class SketchManager
    {
        public readonly Dictionary<Guid, Sketch> Sketches = new Dictionary<Guid, Sketch>();

        public SketchManager()
        {

        }
    }

    public class Sketch
    {
        public Dictionary<string, SketchPage> Pages;
    }

    public class SketchPage
    {
        public IEnumerable<SketchElement> Sketches { get; set; }
    }

    public class SketchElement
    {
        public enum SketchType
        {
            Unknown,
            Line
        }

        public SketchType Type { get; protected set; }
    }

    public class SketchLine : SketchElement
    {
        public SketchLine()
        {
            this.Type = SketchType.Line;
        }

        public Point Start { get; }
        public Point End { get; }

        public SketchLine(Point start, Point end)
        {
            this.Start = start;
            this.End = end;
        }
    }
}
