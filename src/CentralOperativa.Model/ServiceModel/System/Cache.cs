using ServiceStack;

namespace CentralOperativa.ServiceModel.System
{
    public class Cache
    {
        [Route("/system/cache/flush", "GET")]
        public class FlushCache
        {
        }

        [Route("/system/cache/clear/{Pattern}", "GET")]
        public class ClearCache
        {
            public string Pattern { get; set; }
        }
    }
}
