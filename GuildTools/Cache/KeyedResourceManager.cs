using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public class KeyedResourceManager : IKeyedResourceManager
    {
        private ConcurrentDictionary<string, CountedSemaphore> keyLocks;
        SemaphoreSlim accessMutex;

        public KeyedResourceManager()
        {
            this.accessMutex = new SemaphoreSlim(1);
            this.keyLocks = new ConcurrentDictionary<string, CountedSemaphore>();
        }

        public async Task<SemaphoreSlim> AcquireKeyLockAsync(string key)
        {
            await this.accessMutex.WaitAsync();

            CountedSemaphore currentKeyLock;

            if (this.keyLocks.ContainsKey(key))
            {
                currentKeyLock = this.keyLocks[key];
                currentKeyLock.Increment();
            }
            else
            {
                currentKeyLock = new CountedSemaphore();
                this.keyLocks[key] = currentKeyLock;
            }

            var returnSemaphore = currentKeyLock.Semaphore;

            this.accessMutex.Release();

            await returnSemaphore.WaitAsync();

            return returnSemaphore;
        }

        public async Task ReleaseKeyLockAsync(string key)
        {
            await this.accessMutex.WaitAsync();

            if (!this.keyLocks.ContainsKey(key))
            {
                throw new ArgumentException($"{nameof(KeyedResourceManager)} does not contain key '{ key }'.");
            }

            var currentKeyLock = this.keyLocks[key];

            currentKeyLock.Decrement();

            if (currentKeyLock.Count == 0)
            {
                this.keyLocks.Remove(key, out currentKeyLock);
            }

            currentKeyLock.Semaphore.Release();
            this.accessMutex.Release();
        }


        public class CountedSemaphore
        {
            private SemaphoreSlim semaphore;
            private int count;

            public CountedSemaphore()
            {
                this.semaphore = new SemaphoreSlim(1);
                this.count = 1;
            }

            public SemaphoreSlim Semaphore
            {
                get => this.semaphore;
            }

            public int Count
            {
                get => this.count;
            }

            public void Increment()
            {
                this.count++;
            }

            public void Decrement()
            {
                this.count--;
            }
        }
    }
}
