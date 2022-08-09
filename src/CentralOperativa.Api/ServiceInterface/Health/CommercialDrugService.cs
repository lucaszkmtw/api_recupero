using System.Linq;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceModel.Health;
using ServiceStack;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.Health
{
    [Authenticate]
    public class CommercialDrugService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Put(CommercialDrug.Post request)
        {
            return Db.Update((Domain.Health.CommercialDrug)request);
        }

        public object Post(CommercialDrug.Post request)
        {
            request.Id = (int)Db.Insert((Domain.Health.CommercialDrug)request, true);
            return request;
        }

        public object Get(CommercialDrug.Get request)
        {
            var drug = Db.Select<CommercialDrug.QueryResult>(
                Db
                .From<Domain.Health.CommercialDrug>()
                .Join<Domain.Health.CommercialDrug, Domain.Health.Drug>()
                .Join<Domain.Health.CommercialDrug, Domain.Health.DrugPresentation>()
                .LeftJoin<Domain.Health.CommercialDrug, Domain.System.Persons.Person>((cd, p) => cd.ManufacturerId == p.Id)
                .Where(w => w.Id == request.Id)).SingleOrDefault();
            return drug;
        }

        public QueryResponse<CommercialDrug.QueryResult> Get(CommercialDrug.Query request)
        {
            var session = this.Session;

            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            q.LeftJoin<Domain.Health.CommercialDrug, Domain.System.Persons.Person>((cd, p) => cd.ManufacturerId == p.Id);
            return AutoQuery.Execute(request, q);
        }

        public object Get(CommercialDrug.Lookup request)
        {
            var query = Db.From<Domain.Health.CommercialDrug>()
                .Join<Domain.Health.CommercialDrug, Domain.Health.Drug>()
                .LeftJoin<Domain.Health.CommercialDrug, Domain.Health.DrugPresentation>()
                .LeftJoin<Domain.Health.CommercialDrug, Domain.System.Persons.Person>((cd, p) => cd.ManufacturerId == p.Id);

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

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select<ServiceModel.Health.CommercialDrug.QueryResult>(query)
                .Select(x => new LookupItem { Id = x.Id, Text = string.Format("{0} {1} {2} ({3})", x.Name, x.DrugPresentationName, x.PersonName, x.DrugName) }),
                Total = (int)count
            };
            return result;
        }
    }
}