using ServiceStack;

namespace CentralOperativa.ServiceModel.CRM.Contacts
{
    [Route("/crm/contactgroupsbysession", "GET")]
    public class QueryContactGroupBySession : QueryDb<Domain.CRM.Contacts.ContactGroupPermission, QueryContactGroupsResult>
        , IJoin<Domain.CRM.Contacts.ContactGroupPermission, Domain.CRM.Contacts.ContactGroup>
    {
    }

    [Route("/crm/contactgroupsbycontact/{ContactId}", "GET")]
    public class QueryContactGroupByContact : QueryDb<Domain.CRM.Contacts.ContactGroupPermission, QueryContactsGroupCategoryResult>
        , IJoin<Domain.CRM.Contacts.ContactGroupPermission, Domain.CRM.Contacts.ContactGroup>
    {
        public int ContactId { get; set; }
    }

    [Route("/crm/contactgroup/{Id}", "GET")]
    public class GetContactGroup : Domain.CRM.Contacts.ContactGroup
    {
        //public int Id { get; set; }
    }

    [Route("/crm/contactgroup", "POST")]
    [Route("/crm/contactgroup/{Id}", "PUT")]
    public class PostContactGroup : Domain.CRM.Contacts.ContactGroup
    {
    }

    [Route("/crm/contactgroup/{Id}", "DELETE")]
    public class DeleteContactGroup
    {
        public int Id { get; set; }
    }

    public class QueryContactGroupsResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactGroupId { get; set; }
    }

    public class QueryContactsGroupCategoryResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ContactId { get; set; }
        public int ContactGroupId { get; set; }
    }
}