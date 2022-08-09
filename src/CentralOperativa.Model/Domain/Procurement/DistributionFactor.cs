using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Procurement
{
    [Alias("CMP_factores_distribucion")]
    public class DistributionFactor
    {
        [AutoIncrement]
        public int Id { get; set; }

        [Alias("codigo")]
        public string Code { get; set; }

        [Alias("descripcion")]
        public string Description { get; set; }
    }
}