using ServiceStack;
using ServiceStack.Logging;
using Api = CentralOperativa.ServiceModel.Loans;

namespace CentralOperativa.ServiceInterface.Loans
{
    [Authenticate]
    public class LoanInstallmentService : ApplicationService
    {
        public static ILog Log = LogManager.GetLogger(typeof(LoanInstallmentService));

        private readonly IAutoQueryDb _autoQuery;
        private readonly LoanRepository _loanRepository;

        public LoanInstallmentService(
            IAutoQueryDb autoQuery,
            LoanRepository loanRepository)
        {
            _autoQuery = autoQuery;
            _loanRepository = loanRepository;
        }

        public object Get(Api.LoanInstallment.GetLoanInstallments request)
        {
            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            q.OrderBy(li => li.Number);
            var model = _autoQuery.Execute(request, q);
            return model;
        }

        public bool Post(Api.LoanInstallment.PostLoanInstallments request)
        {
            try
            {
                _loanRepository.CalculateLoanInstallments(Db, request.LoanId, request.InstallmentBaseAmount, request.Amount, request.Date.Value, request.Term);
                return true;
            }
            catch
            {
                return false;
            }
        }

    }
}