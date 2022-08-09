using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Domain.Financials.DebtManagement;
using Api = CentralOperativa.ServiceModel.Financials.DebtManagement;
using CentralOperativa.Domain.BusinessPartners;
using System;
//using CentralOperativa.Domain.BusinessDocuments; --ejemplo para tablas estaticas


namespace CentralOperativa.ServiceInterface.Financials.Debtmanagement
{
    [Authenticate]
    public class NormativeService : ApplicationService
    {
        public object Put(Api.PostNormative request)
        {            
            Db.Update((Normative)request);
            return request;         
                      
        }

        public object Post(Api.PostNormative request)
        {
            var normative = Db.Single<Normative>(w => w.Name == request.Name);
            if (normative != null)
            {
                normative.Status = (int)BusinessPartnerStatus.Active;
                normative.Observations=request.Observations;
                Db.Update((Normative)normative);
            }
            else
            {
                request.Id = (int)Db.Insert((Normative)request, true);
            }
            return request;           
        }

        public object Delete(Api.DeleteNormative request)
        {
            var normative = Db.SingleById<Normative>(request.Id);
            normative.Status = (int)BusinessPartnerStatus.Deleted;
            Db.Update((Normative)normative);

            return request;           
        }

        public IAutoQueryDb AutoQuery { get; set; }

        public object Any(Api.QueryNormatives request)
        {
            var query = Db.From<Normative>()
                .Where<Normative>(w => w.Status == (int)BusinessPartnerStatus.Active)
                .OrderByDescending(q => q.Name)
                .Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));

            if (!string.IsNullOrEmpty(request.Name))
                query.Where(q => q.Name.Contains(request.Name));

            if (!string.IsNullOrEmpty(request.Observations))
                query.Where(q => q.Observations.Contains(request.Observations));

            return Db.Select(query);
        }

        public LookupResult Get(Api.LookupNormative request)
        {
            var query = Db.From<Normative>()
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

        public object Get(Api.GetNormative request)
        {
            var model = Db.SingleById<Normative>(request.Id);
            return model;
        }
    }

}