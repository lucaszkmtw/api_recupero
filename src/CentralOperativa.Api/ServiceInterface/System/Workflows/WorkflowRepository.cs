using System.Data;
using System.Linq;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.System
{
    public static class WorkflowRepository
    {
        public static void CopyWorkflow(this IDbConnection db, int workflowId, int tenantId)
        {
            var workflow = db.SingleById<Domain.System.Workflows.Workflow>(workflowId);
            if (workflow != null)
            {
                var originalWorkflowId = workflow.Id;

                workflow.TenantId = tenantId;
                workflow.Id = 0;
                workflow.Id = (int)db.Insert(workflow, true);

                var activities = db.Select(db.From<Domain.System.Workflows.WorkflowActivity>().Where(w => w.WorkflowId == originalWorkflowId));
                foreach (var activity in activities)
                {
                    var originalActivityId = activity.Id;
                    activity.WorkflowId = workflow.Id;

                    activity.Id = (int)db.Insert(activity, true);
                }
            }
        }
    }
}