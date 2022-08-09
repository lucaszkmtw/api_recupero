using ServiceStack;
using ServiceStack.OrmLite;
using Operation = CentralOperativa.ServiceModel.HumanResources.Concept;

namespace CentralOperativa.ServiceInterface.Payroll
{
    [Authenticate]
    public class ConceptServices : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Put(Operation.Post request)
        {
            return Db.Update((Domain.HumanResources.Concept)request);
        }

        public object Post(Operation.Post request)
        {
            request.Id = (int)Db.Insert((Domain.HumanResources.Concept)request, true);
            return request;
        }

        public object Get(Operation.Get request)
        {
            return Db.SingleById<Domain.HumanResources.Concept>(request.Id);
        }

        public QueryResponse<Domain.HumanResources.Concept> Any(Operation.Query request)
        {
            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }
    }
}
