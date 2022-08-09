using System;
using ServiceStack.DataAnnotations;
namespace CentralOperativa.Domain.CRM
{
    [Alias("LeadForms")]
    public class LeadForm
    {
        [AutoIncrement]
        public int Id { get; set; }
        public int FormId { get; set; }
        public int LeadId { get; set; }
    }
}
