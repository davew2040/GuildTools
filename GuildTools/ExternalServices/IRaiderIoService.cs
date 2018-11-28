using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GuildTools.ExternalServices
{
    public interface IRaiderIoService
    {
        Task<string> GetMythicPlusDungeonData(string playerName, string realm);
    }
}
