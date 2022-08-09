using System;
using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System.Workflows
{
    public class WorkflowInstance
    {
        [Route("/system/workflows/workflowinstances/statistics")]
        public class GetWorkflowInstancesStatistics
        {
            public int? WorkflowId { get; set; }

            public string GroupBy { get; set; }

            public bool? Active { get; set; }
        }

        public class WorkflowInstancesStatistics
        {
            public int WorkflowId { get; set; }

            public string WorkflowName { get; set; }

            public int GroupId { get; set; }

            public string GroupName { get; set; }

            public int Items { get; set; }
        }

        public class GetWorkflowInstancesStatisticsResponse
        {
            public Dictionary<int, string> Workflows { get; set; }

            public Dictionary<int, string> Groups { get; set; }

            public List<int[]> Items { get; set; }

            public GetWorkflowInstancesStatisticsResponse()
            {
                Workflows = new Dictionary<int, string>();
                Groups = new Dictionary<int, string>();
                Items = new List<int[]>();
            }
        }

        [Route("/system/workflows/workflowinstances/{Id}", "GET")]
        public class GetWorkflowInstance : IReturn<GetWorkflowInstanceResponse>
        {
            public int Id { get; set; }
        }

        [Route("/system/workflows/workflowinstances", "GET")]
        public class QueryWorkflowInstances : QueryDb<Domain.System.Workflows.WorkflowInstance>
        {
        }

        [Route("/system/workflows/workflowinstances/{WorkflowInstanceGuid}/approve", "POST")]
        public class ApproveWorkflowInstance
        {
            public Guid WorkflowInstanceGuid { get; set; }
        }

        [Route("/system/workflows/workflowinstances/{WorkflowInstanceGuid}/reject", "POST")]
        public class RejectWorkflowInstance
        {
            public Guid WorkflowInstanceGuid { get; set; }
            public int? ReasonId { get; set; }
            public string Reason { get; set; }
        }

        [Route("/system/workflows/workflowinstances/{WorkflowInstanceGuid}/assign", "POST")]
        public class AssignWorkflowInstance
        {
            public Guid WorkflowInstanceGuid { get; set; }
            public int RoleId { get; set; }
        }

        [Route("/system/workflows/workflowinstances/{WorkflowInstanceGuid}/previousstate", "POST")]
        public class SetPreviousStateWorkflowInstance
        {
            public Guid WorkflowInstanceGuid { get; set; }
        }

        [Route("/system/workflows/workflowinstances/{WorkflowInstanceGuid}/setstate", "POST")]
        public class SetStateWorkflowInstance
        {
            public Guid WorkflowInstanceGuid { get; set; }
            public int WorkflowActivityId { get; set; }
        }

        [Route("/system/workflows/workflowinstances/{WorkflowInstanceGuid}/terminate", "POST")]
        public class TerminateWorkflowInstance
        {
            public Guid WorkflowInstanceGuid { get; set; }
            public int? ReasonId { get; set; }
            public string Reason { get; set; }
        }

        [Route("/system/workflows/workflowinstances", "POST")]
        [Route("/system/workflows/workflowinstances/{Id}", "PUT")]
        public class PostWorkflowInstance : Domain.System.Workflows.WorkflowInstance
        {
            public PostWorkflowInstance()
            {
                Tags = new List<Domain.System.Workflows.WorkflowInstanceTag>();
            }

            public List<Domain.System.Workflows.WorkflowInstanceTag> Tags { get; set; }
        }

        [Route("/system/workflows/workflowinstances/{Id}", "DELETE")]
        public class DeleteWorkflowInstance
        {
            public int Id { get; set; }
        }

        [Route("/system/workflows/workflowinstances/lookup", "GET")]
        public class LookupWorkflowInstance : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        public class LookupItemModel
        {
            public int Id { get; set; }
            public string WorkflowCode { get; set; }
        }

        public class GetWorkflowInstanceResponse : Domain.System.Workflows.WorkflowInstance
        {
            public GetWorkflowInstanceResponse()
            {                
                History = new List<WorkflowInstanceHistoryGenericItem>();                

                AssignedRoles = new List<WorkflowInstanceAssignedRole>();

                CanAssignToRoles = new List<Domain.System.Role>();

                CanSetActivities = new List<Domain.System.Workflows.WorkflowActivity>();

                UserPermissions = new List<RoleWithPermission>();

                Approvals = new List<WorkflowInstanceApproval.GetResult>();

                Tags = new List<WorkflowInstanceTag.GetWorkflowInstanceTagResponse>();
            }

            public WorkflowActivity.GetWorkflowActivityResponse CurrentWorkflowActivity { get; set; }

            public Domain.System.Workflows.Workflow Workflow { get; set; }

            public Domain.System.Persons.Person CreatedBy { get; set; }

            public List<WorkflowInstanceAssignedRole> AssignedRoles { get; set; }

            public List<Domain.System.Role> CanAssignToRoles { get; set; }

            public List<Domain.System.Workflows.WorkflowActivity> CanSetActivities { get; set; }

            public List<RoleWithPermission> UserPermissions { get; set; }

            public List<WorkflowInstanceApproval.GetResult> Approvals { get; set; }

            public List<WorkflowInstanceHistoryGenericItem> History { get; set; }

            public List<WorkflowInstanceHistoryGenericItem> HistoryGeneric { get; set; }

            public List<WorkflowInstanceTag.GetWorkflowInstanceTagResponse> Tags { get; set; } 
        }

        public class RoleWithPermission : Domain.System.Role
        {
            public int RoleId { get; set; }
            public string RoleName { get; set; }
            public byte Permission { get; set; }
        }

        public class WorkflowInstanceHistoryItem
        {
            public DateTime CreateDate { get; set; }
            public string Description { get; set; }
            public bool IsTerminated { get; set; }
            public string FromWorkflowActivityName { get; set; }
            public string ToWorkflowActivityName { get; set; }
            public string PersonName { get; set; }
        }

        public class WorkflowInstanceHistoryGenericItem
        {
            public int Type { get; set; }
            public DateTime CreateDate { get; set; }
            public string Description { get; set; }
            public bool IsTerminated { get; set; }
            public string FromWorkflowActivityName { get; set; }
            public string ToWorkflowActivityName { get; set; }
            public string PersonName { get; set; }
            public string Rol { get; set; }
            public string User { get; set; }
            public Boolean IsActive { get; set; }
        }

    }
}
