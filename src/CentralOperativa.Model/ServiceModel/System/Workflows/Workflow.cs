using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System.Workflows
{
    public class Workflow
    {
        [Route("/system/workflows/workflows/{Id}")]
        public class GetWorkflow
        {
            public int Id { get; set; }
        }

        public class GetWorkflowResponse : Domain.System.Workflows.Workflow
        {
            public GetWorkflowResponse()
            {
                this.Activities = new List<Domain.System.Workflows.WorkflowActivity>();
            }

            public List<Domain.System.Workflows.WorkflowActivity> Activities { get; set; }
        }

        [Route("/system/workflows/workflows")]
        public class QueryWorkflows : QueryDb<Domain.System.Workflows.Workflow>
        {
        }

        [Route("/system/workflows/workflows", "POST")]
        [Route("/system/workflows/workflows/{Id}", "PUT")]
        public class PostWorkflow : Domain.System.Workflows.Workflow
        {
        }

        [Route("/system/workflows/workflows/{Id}", "DELETE")]
        public class DeleteWorkflow
        {
            public int Id { get; set; }
        }

        [Route("/system/workflows/workflows/lookup", "GET")]
        public class LookupWorkflow : LookupRequest, IReturn<List<LookupItem>>
        {
        }
    }
}
