using System;
using ServiceStack.DataAnnotations;
namespace CentralOperativa.Domain.CRM
{
    [Alias("LeadFormResponses")]
    public class LeadFormResponse
    {
        [AutoIncrement]
        public int Id { get; set; }
        public int FormResponseId { get; set; }
        public int LeadId { get; set; }
    }
}
