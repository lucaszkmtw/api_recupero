using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.LMS
{
    public class CoursePage
    {
        [Route("/lms/courses/{CourseId}/pages/{Id}")]
        public class GetCoursePage
        {
            public int CourseId { get; set; }
            public int Id { get; set; }
        }

        public class GetCoursePageResponse : Domain.LMS.CoursePage
        {
        }

        [Route("/lms/courses/{CourseId}/pages")]
        public class QueryCoursePages : QueryDb<Domain.LMS.CoursePage>
        {
            public int CourseId { get; set; }
        }

        [Route("/lms/courses/{CourseId}/pages", "POST")]
        [Route("/lms/courses/{CourseId}/pages/{Id}", "PUT")]
        public class PostCoursePage : Domain.LMS.CoursePage
        {
        }

        [Route("/lms/courses/{CourseId}/pages/{Id}", "DELETE")]
        public class DeleteCoursePage
        {
            public int Id { get; set; }
        }

        [Route("/lms/courses/{CourseId}/pages/lookup", "GET")]
        public class LookupCoursePage : LookupRequest, IReturn<List<LookupItem>>
        {
        }
    }
}
