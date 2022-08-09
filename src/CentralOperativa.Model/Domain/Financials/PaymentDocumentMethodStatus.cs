namespace CentralOperativa.Domain.Financials
{
    public enum PaymentDocumentMethodStatus : byte
    {
        Emmited,
        Received,
        Deposited,
        Accredited,
        Canceled,
        Rejected
    }
}
