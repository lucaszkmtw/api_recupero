using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.Health
{
    using Api = ServiceModel.Health;

    [Authenticate]
    public class MedicalPracticeService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Put(Api.PostMedicalPractice request)
        {
            return Db.Update((Domain.Health.MedicalPractice)request);
        }

        public object Post(Api.PostMedicalPractice request)
        {
            request.Id = (int)Db.Insert((Domain.Health.MedicalPractice)request, true);
            return request;
        }

        public Api.GetResponse Get(Api.GetMedicalPractice request)
        {
            var medicalPractice = Db.Select<Api.GetResponse>(Db
                .From<Domain.Health.MedicalPractice>()
                .Where(w => w.Id == request.Id)).SingleOrDefault();
            return medicalPractice;
        }

        public QueryResponse<Api.QueryMedicalPracticeResponse> Get(Api.QueryMedicalPractices request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public object Get(Api.LookupMedicalPractice request)
        {
            var query = Db.From<Domain.Health.MedicalPractice>()
                .Join<Domain.Health.MedicalPractice, Domain.System.Persons.Skill>();

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
                query.Where(q => q.Name.Contains(request.Q) || q.Code.Contains(request.Q));
            }

            var count = Db.Count(query);
            query = query.OrderBy(q => q.Code)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = $"{x.Code} - {x.Name}" }),
                Total = (int)count
            };
            return result;
        }
    }
}