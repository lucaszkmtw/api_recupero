using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("GEN_sector_tipo")]
    public class SectorSectorType
    {
        [AutoIncrement]
        public int Id { get; set; }

        [Alias("sector_id")]
        [References(typeof(Sector))]
        public int SectorId { get; set; }

        [Alias("tipo_sector_id")]
        [References(typeof(SectorType))]
        public int SectorTypeId { get; set; }

        [Alias("usuario_id")]
        [References(typeof(User))]
        public int CreatedById { get; set; }

        [Alias("fecha_creacion")]
        public DateTime CreatedDate { get; set; }
    }
}