using System.Linq;
using CentralOperativa.Domain.Loans;
using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.Loans;

namespace CentralOperativa.ServiceInterface.Loans
{
    [Authenticate]
    public class LoanConceptsService : ApplicationService
    {
        public static ILog Log = LogManager.GetLogger(typeof(LoansService));

        public IAutoQueryDb AutoQuery { get; set; }

        public Api.GetLoanConceptResult Get(Api.GetLoanConcept request)
        {
            var model = Db.Select(Db.From<LoanConcept>()
                    .Where(w => w.Id == request.Id))
                    .SingleOrDefault()
                    .ConvertTo<Api.GetLoanConceptResult>();

            model.Applications = Db.Select(Db.From<LoanConceptDistribution>().Where(w => w.LoanConceptId == request.Id));
            return model;
        }

        public Api.GetLoanConceptResult Post(Api.PostLoanConcept request)
        {
            var model = Db.Insert((LoanConcept) request, true);
            return model.ConvertTo<Api.GetLoanConceptResult>();
        }

        public Api.GetLoanConceptResult Put(Api.PostLoanConcept request)
        {
            Db.Update((LoanConcept)request);
            return request.ConvertTo<Api.GetLoanConceptResult>();
        }
    }
}