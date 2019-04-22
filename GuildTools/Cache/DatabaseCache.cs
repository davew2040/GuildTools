using GuildTools.EF;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public class DatabaseCache<T>
    {
        private GuildToolsContext context;
        private TimeSpan duration;

        public DatabaseCache(GuildToolsContext context, TimeSpan duration)
        {
            this.context = context;
            this.duration = duration;
        }

        public async Task<T> TryGetValueAsync(string key)
        {
            var result = await context.BigValueCache.FirstOrDefaultAsync(x => x.Id == key);

            if (result == null)
            {
                return default(T);
            }

            if (DateTime.Now > result.ExpiresOn)
            {
                context.BigValueCache.Remove(result);
                await context.SaveChangesAsync();

                return default(T);
            }

            return JsonConvert.DeserializeObject<T>(result.Value);
        }

        public async Task InsertValueAsync(string key, T newValue)
        {
            var existingValue = await context.BigValueCache.FirstOrDefaultAsync(x => x.Id == key);
            if (existingValue != null)
            {
                existingValue.ExpiresOn = DateTime.Now + this.duration;
                existingValue.Value = JsonConvert.SerializeObject(newValue);
            }
            else
            {
                context.BigValueCache.Add(new EF.Models.BigValueCache()
                {
                    Id = key,
                    Value = JsonConvert.SerializeObject(newValue),
                    ExpiresOn = DateTime.Now + this.duration
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
