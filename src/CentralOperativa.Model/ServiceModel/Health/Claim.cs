using System;
using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Health
{
    [Route("/health/claims/{Id}", "GET")]
    public class GetClaim : IReturn<GetClaimResponse>
    {
        public int Id { get; set; }
    }

    [Route("/health/claims", "POST")]
    [Route("/health/claims/{Id}", "PUT")]
    public class PostClaim : Domain.Health.Claim
    {
        public PostClaim()
        {
            LinkedPersons = new List<LinkedPerson>();
            LinkedWorkflowInstances = new List<LinkedWorkflowInstance>();
        }

        public string Text { get; set; }

        public ServiceModel.System.Workflows.WorkflowInstance.PostWorkflowInstance WorkflowInstance { get; set; }

        public List<LinkedPerson> LinkedPersons { get; set; }
        public List<LinkedWorkflowInstance> LinkedWorkflowInstances { get; set; }
    }

    [Route("/health/claims", "GET")]
    public class QueryClaims : QueryDb<Domain.Health.Claim, QueryClaimsResponse>
    {
        public byte View { get; set; }
        public string Q { get; set; }
    }

    [Route("/health/claims/lookup", "GET")]
    public class LookupClaim : LookupRequest, IReturn<List<LookupItem>>
        , IJoin<Domain.Health.Claim, Domain.System.Messages.MessageThread>
    {
    }

    public class QueryClaimsResponse
    {
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public int PersonId { get; set; }
        public string PersonName { get; set; }
        public string WorkflowCode { get; set; }
        public int WorkflowActivityId { get; set; }
        public bool WorkflowActivityIsFinal { get; set; }
        public string WorkflowActivityName { get; set; }
        public string Roles { get; set; }
        public int WorkflowInstanceId { get; set; }
        public bool WorkflowInstanceIsTerminated { get; set; }
        public decimal WorkflowInstanceProgress { get; set; }
    }

    public class GetClaimResponse : Domain.Health.Claim
    {
        public GetClaimResponse()
        {
            this.LinkedPersons = new List<LinkedPerson>();
            this.LinkedProcesses = new List<LinkedProcess>();
        }

        public ServiceModel.System.Workflows.WorkflowInstance.GetWorkflowInstanceResponse WorkflowInstance { get; set; }

        public int MessageCount { get; set; }

        public List<LinkedPerson> LinkedPersons { get; set; }

        public List<LinkedProcess> LinkedProcesses { get; set; }
    }

    public class LinkedPerson
    {
        public int? Id { get; set; }
        public System.Persons.Person Person { get; set; }
    }

    public class LinkedWorkflowInstance
    {
        public int? Id { get; set; }
        public System.Workflows.WorkflowInstance.GetWorkflowInstanceResponse WorkflowInstance { get; set; }
    }

    public class LinkedProcess
    {
        public int Id { get; set; }
        public int WorkflowInstanceId { get; set; }
        public DateTime CreateDate { get; set; }
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; }
        public string WorkflowCode { get; set; }
        public string WorkflowDescription { get; set; }
        public int PersonId { get; set; }
        public string PersonName { get; set; }
    }
}