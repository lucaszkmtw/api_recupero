using System;
using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Domain.Financials;

namespace CentralOperativa.Domain.CRM.Contacts
{
    [Alias("Employees")]
    public class Employee
    {        
        [AutoIncrement]
        public int Id { get; set; }
        [References(typeof(Person))]
        public int PersonId { get; set; }
        [References(typeof(Person))]
        public int EmployerId { get; set; }
        [References(typeof(BankAccount))]
        public int BankAccountId { get; set; }
        public decimal Salary { get; set; }
        public int CreatedById { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

    }
}
