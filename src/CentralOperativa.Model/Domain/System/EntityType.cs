using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System
{
    [Alias("EntityTypes")]
    public class EntityType
    {
        [AutoIncrement]
        public short Id { get; set; }

        public string Name { get; set; }

        public string TypeName { get; set; }

        public string TableName { get; set; }
    }
}