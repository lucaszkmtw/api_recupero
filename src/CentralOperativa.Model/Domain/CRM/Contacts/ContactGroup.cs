using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.CRM.Contacts
{
    [Alias("ContactGroups")]
    public class ContactGroup
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(System.Tenant))]
        public int TenantId { get; set; }
        public string Name { get; set; }
        public String Description { get; set; }
    }
}
