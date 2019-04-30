using GuildTools.EF.Models.StoredBlizzardModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.EF.Models
{
    public class GameRegion
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string RegionName { get; set; }

        public virtual IEnumerable<GuildProfile> GuildProfiles { get; set; }
        public virtual IEnumerable<StoredRealm> Realms { get; set; }
    }
}
