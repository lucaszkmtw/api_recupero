using ServiceStack.DataAnnotations;
using System;

namespace CentralOperativa.Domain.System.Workflows
{
    [Alias("WorkflowInstanceTags")]
    public class WorkflowInstanceTag
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(WorkflowInstance))]
        public int? WorkflowInstanceId { get; set; }

        [References(typeof(WorkflowTag))]
        public int? WorkflowTagId { get; set; }

        [References(typeof(User))]
        public int? CreatedById { get; set; }

        public DateTime CreateDate { get; set; }
    }
}
