using System.Collections.Generic;

namespace CentralOperativa.Infraestructure
{
    public class LookupResult
    {
        public IEnumerable<LookupItem> Data { get; set; }

        public int Total { get; set; }
    }

    public class LookupResult<T>
    {
        public IEnumerable<T> Data { get; set; }

        public long Total { get; set; }
    }
}