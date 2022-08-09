using System;
using System.Collections.Generic;
using ServiceStack;

namespace CentralOperativa.ServiceModel.CRM
{
    [Route("/crm/leads/", "GET")]
    public class QueryLeads : QueryDb<Domain.Cms.Forms.FormResponse, QueryLeadsResult>
        , ILeftJoin<Domain.Cms.Forms.FormResponse, Domain.System.Persons.Person>
    {

    }

    [Route("/crm/leadformresponses/", "GET")]
    public class QueryLeadFormResponses : QueryDb<Domain.Cms.Forms.FormResponse, QueryLeadsResult>
        , ILeftJoin<Domain.Cms.Forms.FormResponse, Domain.System.Persons.Person>
    {

    }

    [Route("/crm/leads/{Id}/submitforauthorization", "POST")]
    public class PostLeadSubmitForAuthorization
    {
        public int Id { get; set; }
    }

    [Route("/crm/qualifications/", "GET")]
    public class QueryQualifications : QueryDb<Domain.CRM.Lead, QueryResultQualification>
        , IJoin<Domain.CRM.Lead, Domain.System.Persons.Person>
    {
        public byte View { get; set; }
        public int? PersonId { get; set; }
        public string Q { get; set; }

    }

    [Route("/crm/lead/{FormResponseId}", "GET")]
    public class GetLead : Domain.Cms.Forms.FormResponse
    {
        public int FormResponseId { get; set; }
        public Domain.Cms.Forms.FormResponse FormResponse { get; set; }
        public Domain.Cms.Forms.Form Form { get; set; }
        public System.Persons.Person Person { get; set; }
        public Domain.CRM.Lead Lead { get; set; }
    }


    [Route("/crm/leadauthorization/{Id}", "GET")]
    public class GetLeadAuthorization : Domain.CRM.Lead
    {
        public Domain.CRM.Campaign Campaign { get; set; }
        public System.Persons.Person Person { get; set; }
        public Domain.CRM.Contacts.Employee Employee { get; set; }
        public System.Workflows.WorkflowInstance.GetWorkflowInstanceResponse WorkflowInstance { get; set; }
        public List<Product> Products { get; set; }

        public class Product
        {
            public int Id { get; set; }
            public int ProductId { get; set; }
            public string ProductName { get; set; }
        }

        public List<Form> Forms { get; set; }

        public class Form
        {
            public int Id { get; set; }
            public int FormId { get; set; }
            public string FormName { get; set; }
            public int FormResponseId { get; set; }
        }
    }


    [Route("/crm/leadauthorization/{Id}", "PUT")]
    public class PutLead
    {
        public int Id { get; set; }
        public List<Product> Products { get; set; }

        public class Product
        {
            public int? Id { get; set; }

            public int ProductId { get; set; }

        }

        public List<Form> Forms { get; set; }

        public class Form
        {
            public int? Id { get; set; }

            public int FormId { get; set; }

        }
    }

    public class QueryLeadsResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string CampaignName { get; set; }
        public bool? PersonIsValid { get; set; }
        public int FormId { get; set; }
        public int LeadId { get; set; }
        public int StatusId { get; set; }

    }


    public class QueryResultQualification
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? StartDate { get; set; }
        public string CampaignName { get; set; }
        public bool? PersonIsValid { get; set; }
        public int PersonId { get; set; }
        public int FormId { get; set; }
        public int LeadId { get; set; }
        public decimal WorkflowInstanceProgress { get; set; }
        public string WorkflowActivityName { get; set; }
        public Guid WorkflowInstanceGuid { get; set; }
        public bool? WorkflowActivityIsFinal { get; set; }
        public int WorkflowInstanceId { get; set; }
        public string Roles { get; set; }
        public DateTime WorkflowInstanceCreateDate { get; set; }

    }
}