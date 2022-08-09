using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.LMS
{
    [Alias("CourseCategoryPath")]
    public class CourseCategoryPath : Domain.LMS.CourseCategory
    {
        public string Path { get; set; }

        public int Children { get; set; }
    }
}
