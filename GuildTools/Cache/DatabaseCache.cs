using GuildTools.EF;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public class DatabaseCache : IDatabaseCache
    {
        private const string EmptyStringValue = "null";

        private GuildToolsContext context;

        public DatabaseCache(GuildToolsContext context)
        {
            this.context = context;
        }

        public async Task<CacheResult<T>> TryGetValueAsync<T>(string key)
        {
            var result = await context.BigValueCache.FirstOrDefaultAsync(x => x.Id == key);

            if (result == null)
            {
                return new CacheResult<T>()
                {
                    Found = false
                };
            }

            if (DateTime.Now > result.ExpiresOn)
            {
                context.BigValueCache.Remove(result);
                await context.SaveChangesAsync();

                return new CacheResult<T>()
                {
                    Found = false
                };
            }

            if (result.Value == EmptyStringValue)
            {
                return new CacheResult<T>()
                {
                    Found = true,
                    Result = default(T)
                };
            }

            var deserialized = JsonConvert.DeserializeObject<T>(result.Value);

            return new CacheResult<T>()
            {
                Found = true,
                Result = deserialized
            };
        }

        public async Task InsertValueAsync<T>(string key, T newValue, TimeSpan duration)
        {
            var existingValue = await context.BigValueCache.FirstOrDefaultAsync(x => x.Id == key);
            if (existingValue != null)
            {
                existingValue.ExpiresOn = DateTime.Now + duration;
                existingValue.Value = JsonConvert.SerializeObject(newValue);
            }
            else
            {
                context.BigValueCache.Add(new EF.Models.BigValueCache()
                {
                    Id = key,
                    Value = JsonConvert.SerializeObject(newValue),
                    ExpiresOn = DateTime.Now + duration
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
