using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.HumanResources
{
    [Alias("RRHH_conceptos")]
    public class Concept
    {
        [AutoIncrement]
        public int Id { get; set; }

        [Alias("concepto_numero")]
        public int Number { get; set; }

        [Alias("concepto_descripcion")]
        public string Description { get; set; }

        [Alias("activo")]
        public bool IsActive { get; set; }

        [Alias("calcula_importe")]
        public bool CalculaImporte { get; set; }

        [Alias("calcula_art")]
        public bool CalculaArt { get; set; }

        [Alias("calcula_carga_sindical")]
        public bool CalculaCargaSindical { get; set; }

        [Alias("calcula_contribucion_seguridad_social")]
        public bool CalculaContribucionSeguridadSocial { get; set; }

        [Alias("calcula_plus_vacacional")]
        public bool CalculaPlusVacacional { get; set; }

        [Alias("calcula_sac_proporcional")]
        public bool CalculaSacProporcional { get; set; }

        [Alias("tipo_concepto")]
        public string TipoConcepto { get; set; }

        [Alias("calcula_desayuno")]
        public bool CalculaDesayuno { get; set; }

        [Alias("calcula_almuerzo")]
        public bool CalculaAlmuerzo { get; set; }

        [Alias("calcula_refrigerio")]
        public bool CalculaRefrigerio { get; set; }

        [Alias("calcula_adicional")]
        public bool CalculaAdicional { get; set; }
    }
}