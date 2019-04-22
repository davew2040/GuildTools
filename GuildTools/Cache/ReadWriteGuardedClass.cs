using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
    public class ReadWriteGuardedClass<T> : Cache<T>
    {
        Cache<T> cache;
        ConcurrentDictionary<string, ReaderWriterLockSlim> keyedLocks;
        SemaphoreSlim accessMutex;

        public ReadWriteGuardedClass(Cache<T> cache)
        {
            this.keyedLocks = new ConcurrentDictionary<string, ReaderWriterLockSlim>();
            this.cache = cache;
            this.accessMutex = new SemaphoreSlim(1);
        }

        public override async Task<T> TryGetValueAsync(string key)
        {
            ReaderWriterLockSlim currentKeyLock = this.GetKeyLock(key);

            currentKeyLock.EnterReadLock();

            T value = await this.cache.TryGetValueAsync(key);

            currentKeyLock.ExitReadLock();

            this.RemoveKeyLockIfComplete(key);

            return value;
        }

        public override async Task InsertValueAsync(string key, T newValue)
        {
            ReaderWriterLockSlim currentKeyLock = this.GetKeyLock(key);

            currentKeyLock.EnterWriteLock();

            await this.cache.InsertValueAsync(key, newValue);

            currentKeyLock.ExitWriteLock();

            this.RemoveKeyLockIfComplete(key);
        }

        private ReaderWriterLockSlim GetKeyLock(string key)
        {
            this.accessMutex.Wait();

            ReaderWriterLockSlim currentKeyLock;

            if (this.keyedLocks.ContainsKey(key))
            {
                currentKeyLock = this.keyedLocks[key];
            }
            else
            {
                this.keyedLocks[key] = new ReaderWriterLockSlim();
                currentKeyLock = this.keyedLocks[key];
            }

            this.accessMutex.Release();

            return currentKeyLock;
        }

        private void RemoveKeyLockIfComplete(string key)
        {
            this.accessMutex.Wait();

            var currentKeyLock = this.keyedLocks[key];

            if (currentKeyLock == null)
            {
                this.accessMutex.Release();
                return;
            }

            if (currentKeyLock.WaitingReadCount == 0 && currentKeyLock.WaitingWriteCount == 0)
            {
                this.keyedLocks.Remove(key, out currentKeyLock);
            }

            this.accessMutex.Release();
        }
    }
}
