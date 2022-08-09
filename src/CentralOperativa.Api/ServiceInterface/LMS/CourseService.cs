using System;
using System.Linq;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceModel.Cms;
using ServiceStack;
using ServiceStack.OrmLite;
using Course = CentralOperativa.ServiceModel.LMS.Course;

namespace CentralOperativa.ServiceInterface.LMS
{
    [Authenticate]
    public class CourseService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public Course.PostCourse Put(Course.PostCourse request)
        {
            var existing = Db.SingleById<Domain.LMS.Course>(request.Id);

            if (existing.Name != request.Name || existing.Code != request.Code ||
                existing.Description != request.Description)
            {
                using (var trx = Db.OpenTransaction())
                {
                    try
                    {
                        Db.Update((Domain.LMS.Course) request);

                        if (existing.Name != request.Name)
                        {
                            var section = Db.SingleById<Domain.Cms.Section>(request.SectionId);
                            section.Name = request.Name;
                            Db.Update(section);
                        }

                        trx.Commit();
                    }
                    catch (Exception)
                    {
                        trx.Rollback();
                        throw;
                    }
                }
            }
            return request;
        }

        public Course.PostCourse Post(Course.PostCourse request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    //TODO: Parametrizar la sección root de Courses
                    var section = new PostSection
                    {
                        AllowAnonymous = true,
                        Name = request.Name,
                        ParentId = 1,
                        TemplateId = 1,
                        Published = true
                    };
                    var sectionService = this.TryResolve<Cms.SectionService>();
                    section = (PostSection) sectionService.Post(section);

                    request.TenantId = Session.TenantId;
                    request.SectionId = section.Id;
                    request.CreatedById = Session.UserId;
                    request.CreateDate = DateTime.UtcNow;
                    request.Id = (int) Db.Insert((Domain.LMS.Course) request, true);
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

        public Course.GetCourseResponse Get(Course.GetCourse request)
        {
            var course = Db.SingleById<Domain.LMS.Course>(request.Id).ConvertTo<Course.GetCourseResponse>();
            return course;
        }

        public QueryResponse<Domain.LMS.Course> Get(Course.QueryCourses request)
        {
            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Where(x => x.TenantId == Session.TenantId);
            return AutoQuery.Execute(request, q);
        }

        public LookupResult Get(Course.LookupCourse request)
        {
            var query = Db.From<Domain.LMS.Course>();
            query.Where(x => x.TenantId == Session.TenantId);
            if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where(q => q.Name.Contains(request.Q) || q.Code.Contains(request.Q));
            }

            var count = Db.Count(query);
            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));

            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Name + " (" + x.Code + ")" }),
                Total = (int)count
            };
            return result;
        }
    }
}