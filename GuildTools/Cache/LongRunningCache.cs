using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuildTools.Cache
{
//    public abstract class LongRunningCache<T> where T : class
//    {
//        public enum CachedValueState
//        {
//            Updating,
//            Found
//        }

//        public abstract CachedValue GetCachedValue(string key)
//        {
             
//        }

//        public abstract T RetrieveValue(string key);

//        public class CachedValue
//        {
//            private CachedValue(CachedValueState state, T value)
//            {
//                this.State = state;
//                this.Value = value;
//            }

//            public CachedValueState State { get; }
//            public T Value { get; }
//        }
//    }

//    public abstract class TwoParameterLongRunningCache<T, I1, I2> : LongRunningCache<T>
//    {
//        private Action<I1, I2> action;

//        protected TwoParameterLongRunningCache(Action<I1, I2> action)
//        {
//            this.action = action;
//        }

//        public override T RetrieveValue()
//        {
//            this.action();
//        }

//        public override CachedValue GetCachedValue(I1 one, I2 two)
//        {

//        }
//    }
}
