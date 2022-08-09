using System.Collections.Generic;
using CentralOperativa.Domain.System.Workflows;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System.Workflows
{
    public class WorkflowActivityRole
    {
        [Route("/system/workflows/{WorkflowId}/activities/{WorkflowActivityId}/roles/{Id}")]
        public class Get : IReturn<GetWorkflowActivityRoleResponse>
        {
            public int WorkflowId { get; set; }
            public int WorkflowActivityId { get; set; }
            public int Id { get; set; }
        }

        public class GetWorkflowActivityRoleResponse : Domain.System.Workflows.WorkflowActivityRole
        {
            public string RoleName { get; set; }

            public List<ServiceModel.System.Workflows.WorkflowActivityRolePermission> Permissions { get; set; }

            public GetWorkflowActivityRoleResponse()
            {
                this.Permissions = new List<WorkflowActivityRolePermission>();
            }
        }

        [Route("/system/workflows/{WorkflowId}/activities/{WorkflowActivityId}/roles")]
        public class QueryWorkflowActivityRoles : QueryDb<Domain.System.Workflows.WorkflowActivityRole, QueryWorkflowActivityRolesResponse>, IJoin<Domain.System.Workflows.WorkflowActivityRole, Domain.System.Role>
        {
            public int WorkflowId { get; set; }
            public int WorkflowActivityId { get; set; }
        }

        public class QueryWorkflowActivityRolesResponse : Domain.System.Workflows.WorkflowActivityRole
        {
            public string RoleName { get; set; }
        }

        [Route("/system/workflows/{WorkflowId}/activities/{WorkflowActivityId}/roles", "POST")]
        [Route("/system/workflows/{WorkflowId}/activities/{WorkflowActivityId}/roles/{Id}", "PUT")]
        public class PostWorkflowActivityRole : Domain.System.Workflows.WorkflowActivityRole
        {
            public int WorkflowId { get; set; }

            public List<ServiceModel.System.Workflows.WorkflowActivityRolePermission> Permissions { get; set; }

            public PostWorkflowActivityRole()
            {
                this.Permissions = new List<ServiceModel.System.Workflows.WorkflowActivityRolePermission>();
            }
        }

        [Route("/system/workflows/{WorkflowId}/activities/{WorkflowActivityId}/roles/{Id}", "DELETE")]
        public class DeleteWorkflowActivityRole
        {
            public int WorkflowId { get; set; }
            public int WorkflowActivityId { get; set; }
            public int Id { get; set; }
        }
    }
}
