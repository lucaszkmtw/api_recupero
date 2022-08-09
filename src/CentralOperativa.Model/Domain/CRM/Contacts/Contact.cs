using System;
using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.System.Persons;

namespace CentralOperativa.Domain.CRM.Contacts
{
    [Alias("Contacts")]
    public class Contact
    {
        [AutoIncrement]
        public int Id { get; set; }
        [References(typeof(Person))]
        public int PersonId { get; set; }
    }
}
