using System.Linq;
using CentralOperativa.Infraestructure;
using CentralOperativa.Domain.System;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Domain.Financials.DebtManagement;
using Api = CentralOperativa.ServiceModel.Financials.DebtManagement;
using CentralOperativa.ServiceModel.System.Persons;
using CentralOperativa.Domain.BusinessPartners;

using System;


namespace CentralOperativa.ServiceInterface.Financials.Debtmanagement
{
    [Authenticate]
    public class ProxieService : ApplicationService
    {
        public object Put(Api.PostProxie request)
        {
            Db.Update((Proxie)request);
            return request;
        }
       
        public object Post(Api.PostProxie request)
        {
            var proxie = Db.Single<Proxie>(w => w.PersonId == request.PersonId);
            if (proxie != null)
            {
                proxie.Status = (int)BusinessPartnerStatus.Active;               
                Db.Update((Proxie)proxie);
                request.Id = proxie.Id;
            }
            else
            {
                request.Id = (int)Db.Insert((Proxie)request, true);
            }
            return request;
        }

        public object Delete(Api.DeleteProxie request)
        {

            var dot = Db.SingleById<Proxie>(request.Id);
            dot.Status = (int)BusinessPartnerStatus.Deleted;


            Db.Update((Proxie)dot);

            return request;
        }

        public IAutoQueryDb AutoQuery { get; set; }

        public object Get(Api.GetProxie request)
        {
            var model = Db.SingleById<Proxie>(request.Id);
            return model;
        }



        public QueryResponse<Api.QueryProxieResult> Get(Api.QueryProxies request)
        {
            /*if (request.OrderByDesc == null)
			{
				request.OrderByDesc = "Id";
			}*/

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Where<Proxie>(w => w.Status == (int)BusinessPartnerStatus.Active);
            return AutoQuery.Execute(request, q);
        }



        public object Get(Api.LookupProxie request)
        {
            var query = Db.From<Proxie>()
                        .Join<Proxie, Person>();                        
                        //.Join<Proxie, Domain.BusinessPartners.BusinessPartnerType>();



            if (request.Id.HasValue)
            {
                query.Where(x => x.Id == request.Id.Value);
            }
            else if (request.Ids != null)
            {
                query.Where(x => Sql.In(x.Id, request.Ids));
            }

            query.Where(x => x.Status == (int)BusinessPartnerStatus.Active);


            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select<Api.GetProxieResult>(query).Select(x => new LookupItem { Id = x.Id, Text = x.PersonName }),
                Total = (int)count
            };
            return result;
        }

    }
}