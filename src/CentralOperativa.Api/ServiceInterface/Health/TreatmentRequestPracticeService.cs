using System;
using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using TreatmentRequestPractice = CentralOperativa.ServiceModel.Health.TreatmentRequestPractice;

namespace CentralOperativa.ServiceInterface.Health
{
    [Authenticate]
    public class TreatmentRequestPracticeService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Put(TreatmentRequestPractice.Post request)
        {
            return Db.Update((Domain.Health.TreatmentRequestPractice)request);
        }

        public object Post(TreatmentRequestPractice.Post request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    trx.Commit();
                }
                catch (Exception)
                {
                    trx.Rollback();
                }
            }

            return request;
        }

        public object Get(TreatmentRequestPractice.Get request)
        {
            var treatmentRequestPractice = Db.SingleById<Domain.Health.TreatmentRequestPractice>(request.Id).ConvertTo<TreatmentRequestPractice.GetResponse>();
            return treatmentRequestPractice;
        }

        public QueryResponse<TreatmentRequestPractice.QueryResult> Get(TreatmentRequestPractice.Query request)
        {
            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams())
                .Join<Domain.Health.TreatmentRequest, Domain.Health.MedicalPractice>();
            return AutoQuery.Execute(request, q);
        }

        public LookupResult Get(TreatmentRequestPractice.Lookup request)
        {
            var query = Db
                .From<Domain.Health.TreatmentRequestPractice>()
                .Join<Domain.Health.MedicalPractice>();

            if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where<Domain.Health.MedicalPractice>(q => q.Code.Contains(request.Q) || q.Name.Contains(request.Q));
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));

            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Id.ToString() }),
                Total = (int)count
            };
            return result;
        }
    }
}