using GuildTools.Data.RepositoryModels;
using GuildTools.EF;
using GuildTools.ExternalServices.Blizzard;
using BlizzardUtilities = GuildTools.ExternalServices.Blizzard.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GuildTools.EF.Models;
using EfEnums = GuildTools.EF.Models.Enums;
using Microsoft.AspNetCore.Identity;
using GuildTools.Controllers.Models;
using GuildTools.EF.Models.StoredBlizzardModels;
using EfModels = GuildTools.EF.Models;
using ControllerModels = GuildTools.Controllers.Models;

namespace GuildTools.Data
{
    public class AccountRepository : IAccountRepository
    {
        private GuildToolsContext context;

        public AccountRepository(
            GuildToolsContext context)
        {
            this.context = context;
        }

        public async Task AddOrUpdateUsername(string userId, string username)
        {
            var user = await this.context.UserData.FirstOrDefaultAsync(u => u.UserId == userId);

            if (user == null)
            {
                user = new UserData()
                {
                    UserId = userId,
                    Username = username
                };

                this.context.UserData.Add(user);

                await this.context.SaveChangesAsync();

                return;
            }

            user.Username = username;

            await this.context.SaveChangesAsync();
        }
    }
}
