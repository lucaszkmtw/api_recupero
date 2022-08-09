using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Procurement
{
    [Alias("CMP_distribucion_condicion")]
    public class DistributionCondition
    {
        [AutoIncrement]
        public int Id { get; set; }

        [Alias("distribucion_tipo_id")]
        [References(typeof(DistributionType))]
        public int? DistributionTypeId { get; set; }

        [Alias("distribucion_destino_id")]
        [References(typeof(DistributionDestination))]
        public int? DistributionDestinationId { get; set; }

        [Alias("orden")]
        public int Order { get; set; }

        [Alias("condicion")]
        public string Condition { get; set; }

        [Alias("condicion_alternativa")]
        public string SecondCondition { get; set; }
    }
}