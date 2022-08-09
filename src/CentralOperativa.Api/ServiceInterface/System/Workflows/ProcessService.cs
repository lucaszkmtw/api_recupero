using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using Process = CentralOperativa.ServiceModel.System.Workflows.Process;

namespace CentralOperativa.ServiceInterface.System.Workflows
{
    [Authenticate]
    public class ProcessService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public Domain.System.Workflows.Workflow Get(Process.GetProcess request)
        {
            var solicitud = Db.SingleById<Domain.System.Workflows.Workflow>(request.Id);
            return solicitud;
        }

        public QueryResponse<Domain.System.Workflows.Process> Get(Process.QueryProcesses request)
        {
            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public LookupResult Get(Process.LookupProcess request)
        {
            var query = Db.From<Domain.System.Workflows.Process>();

            if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where(q => q.PersonName.Contains(request.Q));
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));

            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = string.Format("{0}-{1}: {2} - {3}", x.WorkflowCode, x.Id, x.PersonName, x.CreateDate) }),
                Total = (int)count
            };
            return result;
        }
    }
}