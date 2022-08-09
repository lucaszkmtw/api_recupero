using System.Collections.Generic;

namespace CentralOperativa.Infraestructure
{
    public class LookupRequest
    {
        public int? Id { get; set; }
        public List<int> Ids { get; set; }
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string Q { get; set; }
        public string Filter { get; set; }

        public LookupRequest()
        {
            Q = string.Empty;
        }
    }
}
