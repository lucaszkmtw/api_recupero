using ServiceStack;
using System.Collections.Generic;

namespace CentralOperativa.ServiceModel.CRM.Contacts
{
    [Route("/crm/contactsbygroup/{ContactGroupId}", "GET")]
    public class QueryContactByGroup : QueryDb<Domain.CRM.Contacts.Contact, QueryContactsResult>
        , IJoin<Domain.CRM.Contacts.Contact, Domain.System.Persons.Person>
    {
        public int ContactGroupId { get; set; }
        public string Q { get; set; }
    }

    [Route("/crm/contactsbygroup/all", "GET")]
    public class QueryContactByGroupAll : QueryDb<Domain.CRM.Contacts.Contact, QueryContactsResult>
        , IJoin<Domain.CRM.Contacts.Contact, Domain.System.Persons.Person>
    {
        public string Q { get; set; }
    }

    [Route("/crm/contactsbygroupids/{ContactGroupId}", "GET")]
    public class QueryContactByGroupIds : QueryDb<Domain.CRM.Contacts.Contact, QueryContactsResultId>
        , IJoin<Domain.CRM.Contacts.Contact, Domain.System.Persons.Person>
    {
        public int ContactGroupId { get; set; }
        public string Q { get; set; }
    }

    [Route("/crm/contactsbygroupids/all", "GET")]
    public class QueryContactByGroupAllIds : QueryDb<Domain.CRM.Contacts.Contact, QueryContactsResultId>
        , IJoin<Domain.CRM.Contacts.Contact, Domain.System.Persons.Person>
    {
        public string Q { get; set; }
    }

    [Route("/crm/contact/{Id}", "GET")]
    public class QueryContact : Domain.System.Persons.Person
    {
        public QueryContact()
        {
            this.Employee = new Domain.CRM.Contacts.Employee();
            this.Employer = new System.Persons.Person();
            this.BankAccount = new Domain.Financials.BankAccount();
            this.Bank = new Domain.Financials.Bank();
        }
        public System.Persons.Person Person { get; set; }
        public Domain.CRM.Contacts.Employee Employee { get; set; }
        public System.Persons.Person Employer { get; set; }
        public Domain.Financials.BankAccount BankAccount { get; set; }
        public Domain.Financials.Bank Bank { get; set; }
        public int ProcurementsCount { get; set; }
        public int SalesCount { get; set; }
        public int pollsCount { get; set; }
        public int LoansCount { get; set; }
        public int AccountsCount { get; set; }

        public List<ContactBankAccount> BankAccounts { get; set; }

        public class ContactBankAccount
        {
            public int Id { get; set; }
            public int BankAccountId { get; set; }
            public int EmployeeId { get; set; }
            public int BankBranchId { get; set; }
            public string BankBranchName { get; set; }
            public string BankName { get; set; }
            public string BankAccountCode { get; set; }
            public string BankAccountNumber { get; set; }
            public string BankAccountDescription { get; set; }
            public int CurrencyId { get; set; }
        }
    }

    //
    [Route("/crm/employeebankbranch/{Id}", "GET")]
    public class QueryEmployeeBankBranch : Domain.Financials.BankBranch
    {
        public string BankName { get; set; }
    }
    //

    [Route("/crm/contact/{ContactId}/patient", "GET")]
    public class QueryContactPatient : Domain.Health.Patient
    {
        public int ContactId { get; set; }
    }

    [Route("/crm/contact", "POST")]
    [Route("/crm/contact/{Id}", "PUT")]

    public class PostContact : QueryContactsResult
    {
    }

    [Route("/crm/contact/{Id}", "DELETE")]
    public class DeleteContact
    {
        public int Id { get; set; }
    }

    [Route("/crm/contacts/startcampaign", "POST")]
    public class PostContactsStartCampaign
    {
        public int CampaignId { get; set; }
        public List<int> ContactIds { get; set; }
    }


    [Route("/crm/contacts/files", "POST")]
    public class PostContactFile : PostContactFileResult
    {
    }

    public class PostContactFileResult
    {
        public string FileName { get; set; }
        public List<string> Columns { get; set; }
    }

    [Route("/crm/contacts/import", "POST")]
    public class ImportContacts : ImportContactsResult
    {
        public string FileName { get; set; }
        public List<string> Columns { get; set; }
    }
    public class ImportContactsResult
    {
        public int InsertedItemsCount { get; set; }
    }
    public class QueryContactsResult
    {
        public int Id { get; set; }
        public int ContactId { get; set; }
        public int PersonId { get; set; }
        public int ContactGroupId { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Number { get; set; }
        public string EmployerName { get; set; }
        public List<string> GroupNames { get; set; }
        //Empleador, sueldo, cbu nro y banco..
        public int? EmployerId { get; set; }
        public int? BankAccountId { get; set; }
        public decimal? Salary { get; set; }

        public string WebUrl { get; set; }
        public List<ContactBankAccount> BankAccounts { get; set; }

        public class ContactBankAccount
        {
            public int? Id { get; set; }

            public int BankAccountId { get; set; }

            public Domain.Financials.BankAccount BankAccount { get; set; }
        }
    }
    public class QueryContactsResultId
    {
        public int Id { get; set; }

    }
}