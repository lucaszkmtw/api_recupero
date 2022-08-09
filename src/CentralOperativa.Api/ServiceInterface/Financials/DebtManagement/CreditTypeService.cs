using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Domain.Financials.DebtManagement;
using Api=CentralOperativa.ServiceModel.Financials.DebtManagement;
using CentralOperativa.Domain.BusinessPartners;
using System;
using System.Threading.Tasks;
using CentralOperativa.Domain.Catalog;
using CentralOperativa.ServiceModel.Catalog;



namespace CentralOperativa.ServiceInterface.Financials.Debtmanagement
{
    [Authenticate]
    public class CredittypeService : ApplicationService
    {
        public object Put(Api.PostCredittype request)
        {
           Db.Update((CreditType)request);
           return request;
        }

        public object Post(Api.PostCredittype request)
        {
            var credittype = Db.Single<CreditType>(w => w.Code == request.Code);
            if (credittype != null)
            {
                credittype.Status = (int)BusinessPartnerStatus.Active;
                credittype.Name = request.Name;               
                Db.Update((CreditType)credittype);
            }
            else
            {
                request.Id = (int)Db.Insert((CreditType)request, true);
            }
            return request;

        }

        public object Delete(Api.DeleteCreditType request)
        {
            var credittype = Db.SingleById<CreditType>(request.Id);
            credittype.Status = (int)BusinessPartnerStatus.Deleted;
            Db.Update((CreditType)credittype);
       
            return request;
        }

        public IAutoQueryDb AutoQuery { get; set; }

        public object Any(Api.QueryCreditTypes request)
        {
            var query = Db.From<CreditType>()
                .Where<CreditType>(w => w.Status == (int)BusinessPartnerStatus.Active)
                .OrderByDescending(q => q.Name)
                .Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));

            if (!string.IsNullOrEmpty(request.Name))
                query.Where(q => q.Name.Contains(request.Name));

            if (!string.IsNullOrEmpty(request.Code))
                query.Where(q => q.Code.Contains(request.Code));

            return Db.Select(query);
        }
		
		
        public LookupResult Get(Api.LookupCreditType request)
        {
            var query = Db.From<CreditType>()
                .Select(x => new {x.Id, x.Name});

            /*
            if (!string.IsNullOrEmpty(request.Q))
            {
                query = query.Where(q => q.Name.Contains(request.Q) || q.Name.Contains(request.Q));
            }
            */

            if (request.Id.HasValue)
            {
                query.Where(x => x.Id == request.Id.Value);
            }
            else if (request.Ids != null)
            {
                query.Where(x => Sql.In(x.Id, request.Ids));
            }
            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
               .Limit((request.Page.GetValueOrDefault(1) - 1) * request.PageSize.GetValueOrDefault(100), request.PageSize.GetValueOrDefault(100));

			var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Name }),
                Total = (int)count
            };
            return result;
        }
/*
        public object Get(GetCreditType request)
        {
            var model = Db.SingleById<CreditType>(request.Id);
            return model;
        }*/
    }
}