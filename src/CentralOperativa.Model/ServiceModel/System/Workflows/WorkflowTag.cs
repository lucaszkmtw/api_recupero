using ServiceStack;
using System.Collections.Generic;

namespace CentralOperativa.ServiceModel.System.Workflows
{
    public class WorkflowTag
    {

        [Route("/system/workflows/workflows/{WorkflowId}/tags/{Id}", "GET")]
        public class GetWorkflowTag 
        {
            public int WorkflowId { get; set; }
            public int Id { get; set; }
        }


    [Route("/system/workflows/workflows/{WorkflowId}/tags", "GET")]
        public class QueryWorkflowTag : QueryDb<Domain.System.Workflows.WorkflowTag, WorkflowTagResponse>
        {
            public int WorkflowId { get; set; }
        }


        [Route("/system/workflows/workflows/{WorkflowId}/tags/lookup", "GET")]
        public class LookupWorkflowTags
        {
            public int WorkflowId { get; set; }
            public int? ParentId { get; set; }
        }

        [Route("/system/workflows/workflows/{WorkflowId}/tags", "POST")]
        [Route("/system/workflows/workflows/{WorkflowId}/tags/{Id}", "PUT")]
        public class Post : Domain.System.Workflows.WorkflowTag
        {
        }

        [Route("/system/workflows/workflows/{WorkflowId}/tags/{Id}", "DELETE")]
        public class DeleteWorkflowTag
        {
            public int WorkflowId { get; set; }
            public int Id { get; set; }
        }

        [Route("/system/workflows/workflows/{WorkflowId}/tags/{Id}/parent/{ParentId}", "POST")]
        public class UpdateWorkflowTag
        {
            public int WorkflowId { get; set; }
            public int Id { get; set; }
            public int ParentId { get; set; }
        }

        public class LookupWorkflowTagResponse
        {
            public int Id { get; set; }
            public string Text { get; set; }
            public bool HasChildren { get; set; }
        }

        public class WorkflowTagResponse: Domain.System.Workflows.WorkflowTag

    {
            public WorkflowTagResponse()
            {
                this.Items = new List<WorkflowTagResponse>();
            }
           
            public List<WorkflowTagResponse> Items { get; set; }
        }

    }
}
