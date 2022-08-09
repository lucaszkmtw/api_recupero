using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("GrupoDeTrabajo")]
    public class WorkGroup
    {
        public int Id { get; set; }

        [Alias("Nombre")]
        public string Name { get; set; }

        [Alias("Descripcion")]
        public string Description { get; set; }

        [Alias("SysEntidadId")]
        public int CompanyId { get; set; }
    }
}
