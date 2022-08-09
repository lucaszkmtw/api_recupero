using System;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Cms.Forms
{
    [Route("/cms/forms/{FormId}/responses", "GET")]
    public class GetFormResponses : LookupRequest
    {
        public int ContactId { get; set; }
        public int FormId { get; set; }
        public bool ShowOnlyActive { get; set; }
    }

    [Route("/cms/polls/{FormId}/responses", "GET")]
    public class GetPollResponses : QueryDb<Domain.Cms.Forms.FormResponse, GetPollResponsesResult>
        , ILeftJoin<Domain.Cms.Forms.FormResponse, Domain.System.Persons.Person>
    {
        public int FormId { get; set; }
    }

    [Route("/cms/person/{PersonId}/polls", "GET")]
    public class GetPersonPolls : QueryDb<Domain.Cms.Forms.FormResponse, GetPersonPollsResult>
        , IJoin<Domain.Cms.Forms.FormResponse, Domain.Cms.Forms.Form>
    {
        public int PersonId { get; set; }
    }

    [Route("/cms/formresponse/{FormResponseId}", "GET")]
    public class GetFormResponse : Domain.Cms.Forms.FormResponse
    {
        public int FormResponseId { get; set; }

        public Domain.Cms.Forms.FormResponse FormResponse { get; set; }
        public Domain.Cms.Forms.Form Form { get; set; }
        //public Domain.System.Persons.Person Person { get; set; }
        public System.Persons.Person Person { get; set; }
    }

    [Route("/cms/forms/{FormId}/responses/{Id}", "GET")]
    public class GetFormResponseByForm //: IReturn<GetFormResponsesResponse>
    {
        public int Id { get; set; }
        public int ContactId { get; set; }
        public int FormId { get; set; }
        public Guid Guid { get; set; }
    }

    [Route("/cms/forms/{FormId}/responses", "POST")]
    [Route("/cms/forms/{FormId}/responses/{Id}", "PUT")]
    public class PostFormResponse
    {
        public int Id { get; set; }

        public int FormId { get; set; }

        public int? PersonId { get; set; }

        public dynamic Fields { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Email { get; set; }

        public string PostalAddress { get; set; }

        public string Phone { get; set; }

        public byte StatusId { get; set; }
        public System.Persons.PostPerson Person { get; set; }
        public System.Persons.PostPerson Employer { get; set; }
        public Domain.CRM.Contacts.Employee Employee { get; set; }
        public int? LeadId { get; set; }
    }

    [Route("/cms/forms/{FormId}/responses/{Id}", "DELETE")]
    public class DeleteFormResponse : IReturnVoid
    {
        public int Id { get; set; }
        public int FormId { get; set; }
    }

    public class GetPollResponsesResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int StatusId { get; set; }
    }

    public class GetPersonPollsResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string UserName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int StatusId { get; set; }
        public int FormId { get; set; }
        public bool AllowUpdates { get; set; }
    }
}