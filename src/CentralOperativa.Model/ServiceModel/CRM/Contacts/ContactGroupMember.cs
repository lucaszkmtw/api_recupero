using ServiceStack;
using System.Collections.Generic;

namespace CentralOperativa.ServiceModel.CRM.Contacts
{
    [Route("/crm/contactgroupmember", "POST")]
    public class PostContactGroupMember : Domain.CRM.Contacts.ContactGroupMember
    {
    }

    [Route("/crm/contactgroupmember", "DELETE")]
    public class DeleteContactGroupMember : DeleteContactGroupResult
    {
    }

    [Route("/crm/contactgroupmembers", "POST")]
    public class PostContactGroupMembers : MultipleQueryResultPost
    {
    }

    [Route("/crm/contactgroupmembers", "DELETE")]
    public class DeleteContactGroupMembers : MultipleQueryResultPost
    {
    }

    public class DeleteContactGroupResult
    {
        public int ContactId { get; set; }
        public int ContactGroupId { get; set; }
    }

    public class MultipleQueryResultPost
    {
        public List<int> ContactIds { get; set; }
        public int ContactGroupId { get; set; }
    }
}