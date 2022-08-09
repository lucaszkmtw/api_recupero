using System;
using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.System;

namespace CentralOperativa.Domain.CRM.Contacts
{
    [Alias("UserContacts")]
    public class UserContact
    {
        [AutoIncrement]
        public int Id { get; set; }
        [References(typeof(User))]
        public int UserId { get; set; }
        [References(typeof(Contact))]
        public int ContactId { get; set; }
    }
}
