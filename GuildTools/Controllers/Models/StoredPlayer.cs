﻿using System;
using System.Collections.Generic;

namespace GuildTools.Controllers.Models
{
    public class StoredPlayer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Class { get; set; }
        public int GuildId { get; set; }
        public int RealmId { get; set; }
        public StoredRealm Realm { get; set; }
        public StoredGuild Guild { get; set; }
    }
}
