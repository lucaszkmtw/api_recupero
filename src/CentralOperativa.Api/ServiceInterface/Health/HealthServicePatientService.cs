using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.Health
{
    using Api = ServiceModel.Health;

    [Authenticate]
    public class HealthServicePatientService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Put(Api.PostHealthServicePatient request)
        {
            return Db.Update((Domain.Health.HealthServicePatient)request);
        }

        public object Post(Api.PostHealthServicePatient request)
        {
            request.Id = (int)Db.Insert((Domain.Health.HealthServicePatient)request, true);
            return request;
        }

        public object Get(Api.GetHealthServicePatient request)
        {
            var healthServicePatient = Db.Select<Domain.Health.HealthServicePatient>(x => x.PatientId == request.PatientId && x.HealthServiceId == request.HealthServiceId).SingleOrDefault();
            return healthServicePatient;
        }

        public QueryResponse<Api.QueryHealthServicePatientsResult> Get(Api.QueryHealthServicePatients request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Join<Domain.Health.HealthService, Domain.System.Persons.Person>((hs, p) => hs.PersonId == p.Id);
            return AutoQuery.Execute(request, q);
        }

        public object Get(Api.LookupHealthServicePatient request)
        {
            var query = Db.From<Domain.Health.HealthServicePatient>()
                .Join<Domain.System.Persons.Person>();

            if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where<Domain.System.Persons.Person>(q => q.Name.Contains(request.Q));
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select<Api.QueryHealthServicePatientsResult>(query).Select(x => new LookupItem { Id = x.Id, Text = x.PersonName }),
                Total = (int)count
            };
            return result;
        }
    }
}