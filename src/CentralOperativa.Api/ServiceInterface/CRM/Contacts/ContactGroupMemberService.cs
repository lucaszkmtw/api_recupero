using System;
using System.Linq;
using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.CRM.Contacts;

namespace CentralOperativa.ServiceInterface.CRM.Contacts
{
    [Authenticate]
    public class ContactGroupMemberService : ApplicationService
    {
        public static ILog Log = LogManager.GetLogger(typeof(ContactGroupMemberService));

        public IAutoQueryDb AutoQuery { get; set; }

        public Api.PostContactGroupMember Post(Api.PostContactGroupMember request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var groupmember = Db.Select(
                                        Db.From<Domain.CRM.Contacts.ContactGroupMember>()
                                        .Where(c => c.ContactGroupId == request.ContactGroupId && c.ContactId == request.ContactId)
                                      )
                                      .SingleOrDefault();
                    if(groupmember == null)
                    {

                        groupmember = (Domain.CRM.Contacts.ContactGroupMember)request;
                        var _id = (int)Db.Insert(groupmember, true);

                    }

                    trx.Commit();
                    return request;
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
        }

        public object Delete(Api.DeleteContactGroupMember request)
        {
            object model;
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                                        
                    var ContacGroupMembers = Db.From<Domain.CRM.Contacts.ContactGroupMember>()
                        .Select(c => c.Id)
                        .Where(c => c.ContactGroupId == request.ContactGroupId && c.ContactId == request.ContactId);
                    model = Db.Delete<Domain.CRM.Contacts.ContactGroupMember>(ContacGroupMembers);
                    
                    trx.Commit();
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }

            return model;
        }

        public Api.PostContactGroupMembers Post(Api.PostContactGroupMembers request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    foreach (var contactId in request.ContactIds)
                    {
                        var groupmember = Db.Select(
                                        Db.From<Domain.CRM.Contacts.ContactGroupMember>()
                                        .Where(c => c.ContactGroupId == request.ContactGroupId && c.ContactId == contactId)
                                      )
                                      .SingleOrDefault();
                        if (groupmember == null)
                        {
                            groupmember = new Domain.CRM.Contacts.ContactGroupMember();
                            groupmember.ContactGroupId = request.ContactGroupId;
                            groupmember.ContactId = contactId;
                            Db.Insert(groupmember);
                        }

                    }

                    trx.Commit();
                    return request;
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
        }

        public object Delete(Api.DeleteContactGroupMembers request)
        {
            object model = new Domain.CRM.Contacts.ContactGroupMember();
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    /*foreach (var contactId in request.ContactIds)
                    {
                        var ContacGroupMembers = Db.From<Domain.CRM.Contacts.ContactGroupMember>()
                        .Select(c => c.Id)
                        .Where(c => c.ContactGroupId == request.ContactGroupId && c.ContactId == contactId);
                        model = Db.Delete(ContacGroupMembers);
                    }*/

                    var ContacGroupMembers = Db.From<Domain.CRM.Contacts.ContactGroupMember>()
                        .Select(c => c.Id)
                        .Where(c => c.ContactGroupId == request.ContactGroupId && Sql.In( c.ContactId, request.ContactIds) );
                    Db.Delete(ContacGroupMembers);

                    trx.Commit();
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }

            return model;
        }
    }
}