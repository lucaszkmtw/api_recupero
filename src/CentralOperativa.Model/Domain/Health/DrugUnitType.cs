using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("DrugUnitTypes")]
    public class DrugUnitType
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }
    }
}