using System.Collections.Generic;
using CentralOperativa.Domain.System.Workflows;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System.Workflows
{
    public class WorkflowActivity
    {
        [Route("/system/workflows/workflowactivities/{Id}")]
        public class Get : IReturn<GetWorkflowActivityResponse>
        {
            public int Id { get; set; }
        }

        public class GetWorkflowActivityResponse : Domain.System.Workflows.WorkflowActivity
        {
            public List<WorkflowActivityApprovalRule> ApprovalRules { get; set; }

            public GetWorkflowActivityResponse()
            {
                this.ApprovalRules = new List<WorkflowActivityApprovalRule>();
            }
        }

        [Route("/system/workflows/{WorkflowId}/activities")]
        public class Query : QueryDb<Domain.System.Workflows.WorkflowActivity>
            , IJoin<Domain.System.Workflows.WorkflowActivity, Domain.System.Workflows.Workflow>
        {
            public int WorkflowId { get; set; }
            public int ListIndex { get; set; }
            public short WorkflowTypeId { get; set; }
        }

        [Route("/system/workflows/workflowactivities", "POST")]
        [Route("/system/workflows/workflowactivities/{Id}", "PUT")]
        public class Post : Domain.System.Workflows.WorkflowActivity
        {
        }

        [Route("/system/workflows/workflowactivities/{Id}", "DELETE")]
        public class Delete
        {
            public int Id { get; set; }
        }

        [Route("/system/workflows/workflowactivities/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }
    }
}
