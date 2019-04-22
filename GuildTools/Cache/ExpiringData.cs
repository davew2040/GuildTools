using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public class ExpiringData<T>
    {
        public DateTime ExpiresOn { get; }
        public T Data { get; }
        
        public bool IsExpired
        {
            get
            {
                return DateTime.Now > this.ExpiresOn;
            }
        }

        public ExpiringData(DateTime expiresOn, T data)
        {
            this.ExpiresOn = expiresOn;
            this.Data = data;
        }
    }
}
