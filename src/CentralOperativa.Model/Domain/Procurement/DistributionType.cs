using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Procurement
{
    [Alias("CMP_distribucion_tipo")]
    public class DistributionType
    {
        [AutoIncrement]
        public int Id { get; set; }

        [Alias("codigo")]
        public string Code { get; set; }

        [Alias("descripcion")]
        public string Description { get; set; }
    }
}