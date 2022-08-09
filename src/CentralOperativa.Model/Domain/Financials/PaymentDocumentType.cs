using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Financials
{
    [Alias("PaymentDocumentTypes")]
    public class PaymentDocumentType
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }

        public string ShortName { get; set; }

        public string Code { get; set; }
    }
}