using ServiceStack;

namespace CentralOperativa.ServiceModel.System.Workflows
{
    public class WorkflowInstanceTag
    {

        [Route("/system/workflows/workflowInstanceTag/{Id}", "GET")]
        public class GetWorkflowInstanceTag
        {
            public int Id { get; set; }
        }

        public class GetWorkflowInstanceTagResponse : Domain.System.Workflows.WorkflowInstanceTag
        {
            public string WorkflowTagPathName { get; set; }

            public string WorkflowTagPathPath { get; set; }

            public int WorkflowTagPathChildren { get; set; }
        }

        [Route("/system/workflows/workflowInstanceTag", "GET")]
        public class QueryWorkflowInstanceTag : QueryDb<WorkflowInstanceTag>
        {
            public int WorkflowInstanceId { get; set; }
            public int WorkflowTagId { get; set; }
            public int Id { get; set; }
        }

        [Route("/system/workflows/workflowInstanceTag", "POST")]
        public class Post : Domain.System.Workflows.WorkflowInstanceTag
        {
        }

        [Route("/system/workflows/workflowInstanceTag/{Id}", "DELETE")]
        public class DeleteWorkflowInstanceTag
        {
            public int Id { get; set; }
        }
    }
}