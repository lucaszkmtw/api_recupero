using ServiceStack;

namespace CentralOperativa.ServiceModel.CRM
{
    [Route("/crm/person/{Id}", "GET")]
    public class QueryPersons : ServiceModel.System.Persons.Person
    {
        public QueryPersons()
        {
            this.Employee = new Domain.CRM.Contacts.Employee();
            this.Employer = new System.Persons.Person();
            this.BankAccount = new Domain.Financials.BankAccount();
            this.Bank = new Domain.Financials.Bank();
        }

        public Domain.CRM.Contacts.Employee Employee { get; set; }
        public System.Persons.Person Employer { get; set; }
        public Domain.Financials.BankAccount BankAccount { get; set; }
        public Domain.Financials.Bank Bank { get; set; }
        public int ProcurementsCount { get; set; }
        public int SalesCount { get; set; }
        public int PollsCount { get; set; }
        public int LoansCount { get; set; }
    }
}