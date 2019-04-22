using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.EF
{
    public class MigrationsContext
        : IDesignTimeDbContextFactory<GuildToolsContext>
    {
        private const string connectionString =
        "Server=(localdb)\\mssqllocaldb;Database=EfCoreInActionDb;Trusted_Connection=True;MultipleActiveResultSets=true";
        public GuildToolsContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new
                DbContextOptionsBuilder<GuildToolsContext>();
            optionsBuilder.UseSqlServer(connectionString,
                b => b.MigrationsAssembly("DataLayer"));
            return new GuildToolsContext(optionsBuilder.Options);
        }
    }
}
