using System;
using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using CourseCategory = CentralOperativa.ServiceModel.LMS.CourseCategory;

namespace CentralOperativa.ServiceInterface.LMS
{
    [Authenticate]
    public class CourseCategoryService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public CourseCategory.PostCourseCategory Put(CourseCategory.PostCourseCategory request)
        {
            Db.Update((Domain.LMS.CourseCategory)request);
            return request;
        }

        public CourseCategory.PostCourseCategory Post(CourseCategory.PostCourseCategory request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    request.Id = (int) Db.Insert((Domain.LMS.CourseCategory) request, true);
                    trx.Commit();
                }
                catch(Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }

            return request;
        }

        public CourseCategory.GetCourseCategoryResponse Get(CourseCategory.GetCourseCategory request)
        {
            var course = Db.SingleById<Domain.LMS.CourseCategory>(request.Id).ConvertTo<CourseCategory.GetCourseCategoryResponse>();
            return course;
        }

        public QueryResponse<Domain.LMS.CourseCategory> Get(CourseCategory.QueryCourseCategories request)
        {
            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Where(x => x.TenantId == Session.TenantId);
            return AutoQuery.Execute(request, q);
        }

        public LookupResult Get(CourseCategory.LookupCourseCategory request)
        {
            var query = Db.From<Domain.LMS.CourseCategory>();
            if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where(q => q.Name.Contains(request.Q));
            }

            var count = Db.Count(query);
            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));

            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Name}),
                Total = (int)count
            };
            return result;
        }
    }
}