using System.Collections.Generic;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceModel.System.Location;
using ServiceStack;
using System;

namespace CentralOperativa.ServiceModel.System.Persons
{
    [Route("/system/persons/lookup", "GET")]
    public class LookupPerson : LookupRequest, IReturn<List<LookupItem>>
    {
    }

    [Route("/system/persons/{Id}", "GET")]
    public class GetPerson : IReturn<Person>
    {
        public int Id { get; set; }
    }

    [Route("/system/persons/{Id}/nosis", "GET")]
    public class GetPersonNosis
    {
        public int Id { get; set; }
    }

    [Route("/system/persons/code/{Code}", "GET")]
    public class GetPersonByCode : IReturn<Person>
    {
        public string Code { get; set; }
    }

    [Route("/system/persons/email/{Email}", "GET")]
    public class GetPersonByEmail : IReturn<Person>
    {
        public string Email { get; set; }
    }

    public class Person : Domain.System.Persons.Person
    {
        public Person()
        {
            this.Documents = new List<PersonDocument>();
            this.Emails = new List<PersonEmail>();
            this.Phones = new List<PersonPhone>();
            this.Addresses = new List<PersonAddress>();
            this.Fields = new List<PersonField>();
        }

        public List<PersonDocument> Documents { get; set; }
        public List<PersonField> Fields { get; set; }
        public List<PersonEmail> Emails { get; set; }
        public List<PersonPhone> Phones { get; set; }
        public List<PersonAddress> Addresses { get; set; }

        public Reference References { get; set; }
        public int EmployerId { get; set; }
    }

    public class PersonField
    {
        public int Id { get; set; }

        public int FieldId { get; set; }

        public string FieldName { get; set; }

        public string Value { get; set; }
    }

    [Route("/system/persons", "GET")]
    public class QueryPersons : QueryDb<Domain.System.Persons.Person, QueryResult>
    {
        public string[] Codes { get; set; }
        public string[] Datas1 { get; set; }
    }


    [Route("/system/persons/batch", "POST")]
    public class PostPersonBatch : List<PostPersonBatchItem>
    {
    }

    public class PostPersonBatchItem
    {
        public Domain.System.ImportLog ImportLog { get; set; }
        public PostPerson Person { get; set; }
    }

    [Route("/system/persons", "POST")]
    [Route("/system/persons/{Id}", "PUT")]
    public class PostPerson : Domain.System.Persons.Person
    {
        public PostPerson()
        {
            this.Documents = new List<PostPersonDocument>();
            this.Emails = new List<PostPersonEmail>();
            this.Phones = new List<PostPersonPhone>();
            this.Addresses = new List<PostPersonAddress>();
        }

        public List<PostPersonDocument> Documents { get; set; }
        public List<PostPersonEmail> Emails { get; set; }
        public List<PostPersonPhone> Phones { get; set; }
        public List<PostPersonAddress> Addresses { get; set; }
        public int? EmployerId { get; set; }
    }

    public class PostPersonDocument : PersonDocument
    {
        public bool Deleted { get; set; }
    }

    public class PostPersonEmail : PersonEmail
    {
        public bool Deleted { get; set; }
    }

    public class PostPersonPhone : PersonPhone
    {
        public bool Deleted { get; set; }
    }

    public class PostPersonAddress : PersonAddress
    {
        public bool Deleted { get; set; }
    }


    [Route("/system/personsforvalidation", "GET")]
    public class QueryPersonForValidation : QueryDb<Domain.System.Persons.Person, QueryResultForvalidation>
    {
        public string[] Codes { get; set; }
        public string[] Datas1 { get; set; }
        public string PersonPhoneNumber { get; set; }
    }

    [Route("/system/persons/{Id}/validate", "POST")]
    public class ValidatePerson
    {
        public int Id { get; set; }
    }


    [Route("/system/persons/{Id}", "DELETE")]
    public class DeletePerson
    {
        public int Id { get; set; }
    }

    [Route("/system/persons/{PersonId}/formresponses", "GET")]
    public class GetPersonsFormResponsesRequest : QueryDb<Domain.Cms.Forms.FormResponse, PersonsFormResponsesResult>
    {
        public int PersonId { get; set; }
    }

    public class PersonsFormResponsesResult
    {
        public int Id { get; set; }
        public string FormName { get; set; }
        public string UserName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int StatusId { get; set; }
        public int FormId { get; set; }
        public bool AllowUpdates { get; set; }
    }

    public class QueryResult
    {
        public int Id { get; set; }
        public bool IsOrganization { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Data1 { get; set; }
    }


    public class QueryResultForvalidation
    {
        public int Id { get; set; }
        public bool IsOrganization { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Data1 { get; set; }
        public string PersonPhoneNumber { get; set; }
    }

    public class PersonAddress : Domain.System.Persons.PersonAddress
    {
        public Address.GetAddressResult Address { get; set; }
    }
}
