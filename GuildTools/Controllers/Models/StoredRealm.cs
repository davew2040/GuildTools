﻿using System;
using System.Collections.Generic;

namespace GuildTools.Controllers.Models
{
    public class StoredRealm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Slug { get; set; }
        public int RegionId { get; set; }
        public GameRegion Region { get; set; }
    }
}
