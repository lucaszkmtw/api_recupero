using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.LMS
{
    [Alias("CoursePagePath")]
    public class CoursePagePath : Domain.LMS.CoursePage
    {
        public string Path { get; set; }

        public int Children { get; set; }
    }
}
