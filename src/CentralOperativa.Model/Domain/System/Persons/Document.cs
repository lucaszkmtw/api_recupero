using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("PersonaDocumento")]
    public class Document
    {
        [AutoIncrement]
        public int Id { get; set; }

        [Alias("PersonaId")]
        [References(typeof(Person))]
        public int PersonId { get; set; }

        [Alias("SysEntidadId")]
        public int TenantId { get; set; }

        [Alias("Documento")]
        public string Number { get; set; }

        [Alias("DocumentoTiposId")]
        public int DocumentTypeId { get; set; }
    }
}
