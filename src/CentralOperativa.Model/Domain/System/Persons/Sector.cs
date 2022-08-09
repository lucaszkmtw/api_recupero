using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Persons
{
    [Alias("GEN_Sector")]
    public class Sector
    {
        [AutoIncrement]
        public int Id { get; set; }

        [Alias("sector_id")]
        [References(typeof(Sector))]
        public int? ParentId { get; set; }

        [Alias("codigo")]
        public string Code { get; set; }

        [Alias("descripcion")]
        public string Description { get; set; }

        [Alias("usuario_id")]
        [References(typeof(User))]
        public int CreatedById { get; set; }

        [Alias("fecha_creacion")]
        public DateTime CreatedDate { get; set; }

        [Alias("configura_distribucion_rrhh")]
        public bool ConfigPayrollDistribution { get; set; }
    }
}