using System.Linq;
using CentralOperativa.ServiceModel.System;
using ServiceStack;

namespace CentralOperativa.ServiceInterface.System
{
    [Authenticate]
    public class CacheService: ApplicationService
    {
        private readonly ServiceStack.Redis.IRedisClientsManager redisClientManager;

        public CacheService(ServiceStack.Redis.IRedisClientsManager redisClientManager)
        {
            this.redisClientManager = redisClientManager;
        }

        public object Get(Cache.FlushCache request)
        {
            redisClientManager?.GetClient().FlushDb();
            return true;
        }

        public object Get(Cache.ClearCache request)
        {
            var keys = base.Cache.GetKeysByPattern("urn:" + request.Pattern + "*").ToList();
            foreach (var key in keys)
            {
                Request.RemoveFromCache(base.Cache, key);
            }
            return true;
        }
    }
}
