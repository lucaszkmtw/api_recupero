using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System
{
    [Alias("Modules")]
    public class Module
    {
        [AutoIncrement]
        public byte Id { get; set; }

        public string Name { get; set; }

        public byte ListIndex { get; set; }
    }
}