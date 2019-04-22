using GuildTools.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Data
{
    public class DataRepository : IDataRepository
    {
        GuildToolsContext context;

        public DataRepository(GuildToolsContext context)
        {
            this.context = context;
        }

        public async Task<string> SimpleGetAsync()
        {
            return (await this.context.GuildProfile.FindAsync(1)).GuildName;
        }
    }
}
