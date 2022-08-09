using System;
using System.Threading.Tasks;
using System.Data;
using System.Linq;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Domain.System.Workflows;
using CentralOperativa.Infraestructure;
using Api = CentralOperativa.ServiceModel.System.Workflows.WorkflowActivity;

namespace CentralOperativa.ServiceInterface.System.Workflows
{
    public class WorkflowActivityRepository
    {
        private readonly IAutoQueryDb _autoQuery;

        public WorkflowActivityRepository(IAutoQueryDb autoQuery)
        {
            _autoQuery = autoQuery;
        }

        public async Task<Api.GetWorkflowActivityResponse> GetWorkflowActivity(IDbConnection db, int id)
        {
            var workflowActivity = (await db.SingleByIdAsync<WorkflowActivity>(id)).ConvertTo<Api.GetWorkflowActivityResponse>();
            workflowActivity.ApprovalRules = await db.SelectAsync<WorkflowActivityApprovalRule>(w => w.WorkflowActivityId == workflowActivity.Id);
            return workflowActivity;
        }

        public async Task<Api.GetWorkflowActivityResponse> GetWorkflowActivity(IDbConnection db, Session session, int listIndex, short workflowTypeId)
        {
            var workflowActivity = (await db.SelectAsync<Api.GetWorkflowActivityResponse>(db.From<WorkflowActivity>().Join<Workflow>()
                .Where(w => w.ListIndex == listIndex)
                .And<Workflow>(w => w.TypeId == workflowTypeId)
                .And<Workflow>(w => w.TenantId == session.TenantId)))
            .SingleOrDefault();
            if (workflowActivity == null)
            {
                throw new ApplicationException("Invalid workflow definition");
            }

            workflowActivity.ApprovalRules = await db.SelectAsync<WorkflowActivityApprovalRule>(w => w.WorkflowActivityId == workflowActivity.Id);
            return workflowActivity;
        }

        public QueryResponse<WorkflowActivity> QueryWorkflowActivities(IDbConnection db, Api.Query request)
        {
            var q = _autoQuery.CreateQuery(request, HostContext.GetCurrentRequest().GetRequestParams());
            return _autoQuery.Execute(request, q);
        }
    }
}