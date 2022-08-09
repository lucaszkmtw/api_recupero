using System.Collections.Generic;

namespace CentralOperativa.Infraestructure
{
    public class QueryRequestBase<T>
    {
        public virtual int? Skip { get; set; }
        public virtual int? Take { get; set; }
        public virtual string OrderBy { get; set; }
        public virtual string OrderByDesc { get; set; }
        public virtual string Include { get; set; }
        public virtual string Fields { get; set; }
        public virtual Dictionary<string, string> Meta { get; set; }
        public string Q { get; set; }
    }
}
