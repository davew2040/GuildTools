using GuildTools.EF.Models.StoredBlizzardModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Controllers.Models
{
    public class GameRegion
    {
        public int Id { get; set; }
        public string RegionName { get; set; }
    }
}
