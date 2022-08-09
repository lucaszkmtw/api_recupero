using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.CRM.Contacts
{
    [Alias("ContactGroupMembers")]
    public class ContactGroupMember
    {
        [AutoIncrement]
        public int Id { get; set; }
        [References(typeof(ContactGroup))]
        public int ContactGroupId { get; set; }
        [References(typeof(Contact))]
        public int ContactId { get; set; }
    }
}
