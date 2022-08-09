using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Domain.Financials.DebtManagement;
using Api = CentralOperativa.ServiceModel.Financials.DebtManagement;
using CentralOperativa.Domain.BusinessPartners;
using CentralOperativa.ServiceModel.BusinessPartners;
using System;
//using CentralOperativa.Domain.BusinessDocuments; --ejemplo para tablas estaticas


namespace CentralOperativa.ServiceInterface.Financials.Debtmanagement
{
    [Authenticate]
    public class OrganismtypeService : ApplicationService
    {
        public object Put(Api.PostOrganismtype request)
        {
            Db.Update((OrganismType)request);
                     
            return request;
        }

        public object Post(Api.PostOrganismtype request)
        {

            var organismtype = Db.Single<OrganismType>(w => w.BusinessPartnerTypeId == request.BusinessPartnerTypeId);
            if (organismtype != null)
            {
                organismtype.Status = (int)BusinessPartnerStatus.Active;
                organismtype.Name = request.Code;
                organismtype.Name = request.Name;
                Db.Update((OrganismType)organismtype);
            }else
            {
                request.Id = (int)Db.Insert((OrganismType)request, true);               
            }
            return request;            
        }

        public object Delete(Api.DeleteOrganismType request)
        {
            var delorganismtype = Db.SingleById<OrganismType>(request.Id);
            delorganismtype.Status = (int)BusinessPartnerStatus.Deleted;
            Db.Update((OrganismType)delorganismtype);
            return request;
        }

        public IAutoQueryDb AutoQuery { get; set; }
     
        public object Any(Api.QueryOrganismTypes request)
        {
            var query = Db.From<OrganismType>()
                .Join<Organism, CentralOperativa.Domain.BusinessPartners.BusinessPartnerType>()
                .Where<OrganismType>(w => w.Status == (int)BusinessPartnerStatus.Active)
                .OrderByDescending(q => q.Name)
                .Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));
           
           /*
            if (!string.IsNullOrEmpty(request.Name))
                query.Where(q => q.Name.Contains(request.Name));

            if (!string.IsNullOrEmpty(request.Code))
                query.Where(q => q.Code.Contains(request.Code));*/

            return Db.Select(query);
        }

        public object Get(Api.GetOrganismType request)
        {
            var organism = Db.SingleById<OrganismType>(request.Id);
            var model = organism.ConvertTo<OrganismType>();
            return model;
        }

        public QueryResponse<Api.QueryOrganismTypeResult> Get(Api.QueryOrganismTypes request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Where<OrganismType>(w => w.Status == (int)BusinessPartnerStatus.Active);
            return AutoQuery.Execute(request, q);
        }

        public LookupResult Get(Api.LookupOrganismType request)
        {
            var query = Db.From<OrganismType>()
                .Select(x => new { x.Id, x.Name });

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
        public object Get(Api.GetOrganismType request)
        {
            var model = Db.SingleById<OrganismType>(request.Id);
            return model;
        }*/
    }
}