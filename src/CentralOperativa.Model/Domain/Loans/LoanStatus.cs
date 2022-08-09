using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Loans
{
    [Flags, EnumAsInt]
    public enum LoanStatus : byte
    {
        Pending = 1, // Pendiente
        InEvaluation = 2, // En evaluación
        Approved = 3, // Aprobado
        PendingReception = 4, // Pendiente de recepción de legajo
        Portfolio = 5, // En cartera
        ToExecute = 6, // A Liquidar
        Suspended = 7, // Suspendido
        Cancelled = 8, // Cancelado (Cumplido)
        Voided = 9, // Rechazado
        Paid = 10, //Pagado
        PendingPayment = 11 //A pagar
    }
}
