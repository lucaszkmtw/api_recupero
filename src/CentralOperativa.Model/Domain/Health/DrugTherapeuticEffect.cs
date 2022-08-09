using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("DrugTherapeuticEffects")]
    public class DrugTherapeuticEffect
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }
    }
}