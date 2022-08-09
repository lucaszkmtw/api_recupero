using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Domain.Financials.DebtManagement;
using Api = CentralOperativa.ServiceModel.Financials.DebtManagement;
using CentralOperativa.Domain.BusinessPartners;
using System;
using System.Threading.Tasks;

namespace CentralOperativa.ServiceInterface.Financials.Debtmanagement
{
    [Authenticate]
    public class PrescriptionService : ApplicationService
    {
        public object Put(Api.PostPrescription request)
        {
            Db.Update((Prescription)request);
            return request;
        }

        public object Post(Api.PostPrescription request)
        {
            var prescription = Db.Single<Prescription>(w => w.NormativeId == request.NormativeId);
            if (prescription != null)
            {
                prescription.Status = (int)BusinessPartnerStatus.Active;
                prescription.NumberOfDays = request.NumberOfDays;
                prescription.Suspends = request.Suspends;
                prescription.Interrupt = request.Interrupt;
                prescription.Observations = request.Observations;
                prescription.StartDate = request.StartDate;
                prescription.EndDate = request.EndDate;
                Db.Update((Prescription)prescription);
            }
            else
            {
                request.Id = (int)Db.Insert((Prescription)request, true);
            }
            return request;
        }

        public object Delete(Api.DeletePrescription request)
        {
            var prescription = Db.SingleById<Prescription>(request.Id);
            prescription.Status = (int)BusinessPartnerStatus.Deleted;
            Db.Update((Prescription)prescription);

            return request;
        }
        /*
        public async Task<Api.GetResponseLaw> Get(Api.GetLaw request)
        {
            var law = (await Db.SelectAsync<Api.GetResponseLaw>(Db.From<Law>()
                .Where(w => w.Id == request.Id))).SingleOrDefault();
            return law;
        }
        */
        public IAutoQueryDb AutoQuery { get; set; }

        public object Any(Api.QueryPrescriptions request)
        {
            var query = Db.From<Domain.Financials.DebtManagement.Prescription>()
                .Join<Prescription, Normative>()
                .Where<Prescription>(w => w.Status == (int)BusinessPartnerStatus.Active)
                //.OrderByDescending(q => q.Name)
                .Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));
            /*
            if (!string.IsNullOrEmpty(request.Name))
                query.Where(q => q.Name.Contains(request.Name));

            if (!string.IsNullOrEmpty(request.Code))
                query.Where(q => q.Code.Contains(request.Code));
                */

            return Db.Select(query);
        }

        public QueryResponse<Api.QueryPrescriptionResult> Get(Api.QueryPrescriptions request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public LookupResult Get(Api.LookupPrescription request)
        {
            var query = Db.From<Prescription>()
                .Select(x => new { x.Id});


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
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id }),
                Total = (int)count
            };
            return result;
        }
      
        public object Get(Api.GetPrescription request)
        {
            var model = Db.SingleById<Prescription>(request.Id);
            return model;
        }
    }
}