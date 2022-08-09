namespace CentralOperativa.ServiceModel.Loans
{
    public class BancoDeComercioScoringRequest
    {
        public string Nombre { get; set; }

        /// <summary>
        /// 1 -> LC
        /// 2 -> LE
        /// 3 -> DNI
        /// 9 -> CUIT
        /// </summary>
        public string TipoDocumento { get; set; }

        public string NroDoc { get; set; }

        public string FechaNacimiento { get; set; }

        public string CodigoPostal { get; set; }

        public string Localidad { get; set; }

        public string AntiguedadLaboral { get; set; }

        public string ImporteCuota { get; set; }

        public string Sexo { get; set; }

        public string ConstanciaIngresos { get; set; }

        public string ImporteIngresos { get; set; }

        public string Nacionalidad { get; set; }

        public string EsCliente { get; set; }

        public string Producto { get; set; }

        public string Subproducto { get; set; }
    }
}
