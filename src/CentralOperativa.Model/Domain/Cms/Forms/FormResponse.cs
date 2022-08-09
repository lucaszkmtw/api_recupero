using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Cms.Forms
{
    [Alias("FormResponses")]
    public class FormResponse
    {
        [AutoIncrement]
        public int Id { get; set; }

        public int FormId { get; set; }

        public int? PersonId { get; set; }

        public string Code { get; set; }

        public Guid Guid { get; set; }

        public string ClientIp { get; set; }

        public byte StatusId { get; set; }

        public dynamic Fields { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int CreatedById { get; set; }
    }
}