using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("GEN_tipo_sector")]
    public class SectorType
    {
        [AutoIncrement]
        public int Id { get; set; }

        [Alias("codigo")]
        public string Code { get; set; }

        [Alias("descripcion")]
        public string Description { get; set; }
    }
}