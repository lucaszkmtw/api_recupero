using CentralOperativa.Domain.System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.LMS
{
    [Alias("CourseCategories")]
    public class CourseCategory
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Tenant))]
        public int TenantId { get; set; }

        [References(typeof(CourseCategory))]
        public int? ParentId { get; set; }

        public string Name { get; set; }
    }
}
