using System;
using ServiceStack.DataAnnotations;
namespace CentralOperativa.Domain.CRM
{
    [Alias("LeadProducts")]
    public class LeadProduct
    {
        [AutoIncrement]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int LeadId { get; set; }
    }
}
