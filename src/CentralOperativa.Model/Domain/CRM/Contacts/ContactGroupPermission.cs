using System;
using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.System;

namespace CentralOperativa.Domain.CRM.Contacts
{
    [Alias("ContactGroupPermissions")]
    public class ContactGroupPermission
    {
        [AutoIncrement]
        public int Id { get; set; }
        [References(typeof(ContactGroup))]
        public int ContactGroupId { get; set; }
        [References(typeof(Role))]
        public int RoleId { get; set; }
        [References(typeof(User))]
        public int UserId { get; set; }
        public int Permission { get; set; }
    }
}
