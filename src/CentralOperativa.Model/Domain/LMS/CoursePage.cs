using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.LMS
{
    [Alias("CoursePages")]
    public class CoursePage
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Course))]
        public int CourseId { get; set; }

        [References(typeof(CoursePage))]
        public int? ParentId { get; set; }

        public string Title { get; set; }

        public string Summary { get; set; }

        public string Text { get; set; }
    }
}
