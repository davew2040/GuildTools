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
            "Data Source=(LocalDb)\\MSSQLLocalDB;Initial Catalog=GuildTools;Integrated Security=True";
        public GuildToolsContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new
                DbContextOptionsBuilder<GuildToolsContext>();
            optionsBuilder.UseSqlServer(connectionString,
                b => b.MigrationsAssembly("GuildTools"));
            return new GuildToolsContext(optionsBuilder.Options);
        }
    }
}
