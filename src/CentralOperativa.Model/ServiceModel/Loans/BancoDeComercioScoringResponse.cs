namespace CentralOperativa.ServiceModel.Loans
{
    public class BancoDeComercioScoringReponse
    {
        /// <summary>
        /// Número interno de solicitud
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Resultado de la operación, debe ser siempre OK
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Resultado informado por veraz
        /// </summary>
        public string Resultado { get; set; }

        /// <summary>
        /// Descripción otorgada por veraz
        /// </summary>
        public string Descripcion { get; set; }

        /// <summary>
        /// Puntaje
        /// </summary>
        public string Score { get; set; }

        /// <summary>
        /// Relación cuota-ingreso
        /// </summary>
        public string RCI { get; set; }
    }
}
