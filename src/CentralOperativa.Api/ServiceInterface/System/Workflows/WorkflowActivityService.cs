using System;
using System.Linq;
using System.Threading.Tasks;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using WorkflowActivity = CentralOperativa.ServiceModel.System.Workflows.WorkflowActivity;

namespace CentralOperativa.ServiceInterface.System.Workflows
{
    [Authenticate]
    public class WorkflowActivityService : ApplicationService
    {
        private readonly WorkflowActivityRepository _workflowActivityRepository;

        public WorkflowActivityService(WorkflowActivityRepository workflowActivityRepository)
        {
            _workflowActivityRepository = workflowActivityRepository;
        }

        public object Put(WorkflowActivity.Post request)
        {
            return Db.Update((Domain.System.Workflows.WorkflowActivity)request);
        }

        public object Post(WorkflowActivity.Post request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    request.Id = (int) Db.Insert((Domain.System.Workflows.WorkflowActivity) request, true);
                    trx.Commit();
                }
                catch(Exception)
                {
                    trx.Rollback();
                }
            }

            return request;
        }

        public async Task<WorkflowActivity.GetWorkflowActivityResponse> Get(WorkflowActivity.Get request)
        {
            return await _workflowActivityRepository.GetWorkflowActivity(Db, request.Id);
        }

        public QueryResponse<Domain.System.Workflows.WorkflowActivity> Get(WorkflowActivity.Query request)
        {
            return _workflowActivityRepository.QueryWorkflowActivities(Db, request);
        }

        public LookupResult Get(WorkflowActivity.Lookup request)
        {
            var query = Db.From<Domain.System.Workflows.WorkflowActivity>();

            if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where(q => q.Name.Contains(request.Q));
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));

            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Name }),
                Total = (int)count
            };
            return result;
        }
    }
}