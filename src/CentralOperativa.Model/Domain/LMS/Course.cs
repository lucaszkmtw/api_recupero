using System;
using CentralOperativa.Domain.System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.LMS
{
    [Alias("Courses")]
    public class Course
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Tenant))]
        public int TenantId { get; set; }

        [References(typeof(Cms.Section))]
        public int SectionId { get; set; }

        [References(typeof(User))]
        public int CreatedById { get; set; }

        public DateTime CreateDate { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}