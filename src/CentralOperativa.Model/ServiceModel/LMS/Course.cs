using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.LMS
{
    public class Course
    {
        [Route("/lms/courses/{Id}")]
        public class GetCourse
        {
            public int Id { get; set; }
        }

        public class GetCourseResponse : Domain.LMS.Course
        {
        }

        [Route("/lms/courses")]
        public class QueryCourses : QueryDb<Domain.LMS.Course>
        {
        }

        [Route("/lms/courses", "POST")]
        [Route("/lms/courses/{Id}", "PUT")]
        public class PostCourse : Domain.LMS.Course
        {
        }

        [Route("/lms/courses/{Id}", "DELETE")]
        public class DeleteCourse
        {
            public int Id { get; set; }
        }

        [Route("/lms/courses/lookup", "GET")]
        public class LookupCourse : LookupRequest, IReturn<List<LookupItem>>
        {
        }
    }
}
