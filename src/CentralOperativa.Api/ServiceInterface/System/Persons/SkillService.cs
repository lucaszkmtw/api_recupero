using System.Linq;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceModel.System.Persons;
using ServiceStack;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.System.Persons
{
    [Authenticate]
    public class SkillService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Put(Skill.Post request)
        {
            return Db.Update((Domain.System.Persons.Skill)request);
        }

        public object Post(Skill.Post request)
        {
            request.Id = (int)Db.Insert((Domain.System.Persons.Skill)request, true);
            return request;
        }

        public object Get(Skill.Get request)
        {
            var speciality = Db.SingleById<Domain.System.Persons.Skill>(request.Id);
            return speciality;
        }

        public QueryResponse<Skill.QueryResult> Get(Skill.Query request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public object Get(Skill.Lookup request)
        {
            var query = Db.From<Domain.System.Persons.Skill>()
                .Select(x => new { x.Id, x.Name });

            if (request.Id.HasValue)
            {
                query.Where(w => w.Id == request.Id);

            }
            else if (request.Ids != null)
            {
                query.Where(w => Sql.In(w.Id, request.Ids));
            }
            else if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where(q => q.Name.Contains(request.Q));
            }

            var count = Db.Count(query);

            //query = query.OrderByDescending(q => q.Id)
            //.Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));

            query = query.OrderByDescending(q => q.Id)
                           .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));

            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Name }),
                Total = (int)count
            };
            return result;
        }
    }
}