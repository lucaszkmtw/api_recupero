using ServiceStack.DataAnnotations;
using System;

namespace CentralOperativa.Domain.BusinessDocuments
{
    [Alias("TmpMigracionBD")]
    public class BusinessDocumentMigration
    {

        public string ORGANISMO { get; set; }

        public string TIPODECREDITO { get; set; }

        public string NUMDEEXPEDIENTE  { get; set; }

        public string NOMBRE { get; set; }

        public decimal MONTOORIGINAL { get; set; }

        public DateTime FECHADENOTIFICACION { get; set; }

        public DateTime FECHADEPRESCRIPCION { get; set; }

        public DateTime INGRESODPGYRCF { get; set; }
    }
}