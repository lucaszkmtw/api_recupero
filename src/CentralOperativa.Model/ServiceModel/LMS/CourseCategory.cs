using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.LMS
{
    public class CourseCategory
    {
        [Route("/lms/coursecategories/{Id}")]
        public class GetCourseCategory
        {
            public int Id { get; set; }
        }

        public class GetCourseCategoryResponse : Domain.LMS.CourseCategory
        {
        }

        [Route("/lms/coursecategories")]
        public class QueryCourseCategories : QueryDb<Domain.LMS.CourseCategory>
        {
        }

        [Route("/lms/coursecategories", "POST")]
        [Route("/lms/coursecategories/{Id}", "PUT")]
        public class PostCourseCategory : Domain.LMS.CourseCategory
        {
        }

        [Route("/lms/coursecategories/{Id}", "DELETE")]
        public class DeleteCourseCategory
        {
            public int Id { get; set; }
        }

        [Route("/lms/coursecatgories/lookup", "GET")]
        public class LookupCourseCategory : LookupRequest, IReturn<List<LookupItem>>
        {
        }
    }
}
