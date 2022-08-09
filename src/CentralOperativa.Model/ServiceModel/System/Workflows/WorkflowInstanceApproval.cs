namespace CentralOperativa.ServiceModel.System.Workflows
{
    public class WorkflowInstanceApproval
    {
        public class GetResult : Domain.System.Workflows.WorkflowInstanceApproval
        {
            public string UserName { get; set; }

            public string RoleName { get; set; }
        }
    }
}
