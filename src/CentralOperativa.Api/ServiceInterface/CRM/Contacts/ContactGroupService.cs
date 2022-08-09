using System;
using System.Linq;
using CentralOperativa.Domain.CRM.Contacts;
using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.CRM.Contacts;

namespace CentralOperativa.ServiceInterface.CRM.Contacts
{
    [Authenticate]
    public class ContactGroupService : ApplicationService
    {
        public static ILog Log = LogManager.GetLogger(typeof(ContactGroupService));

        public IAutoQueryDb AutoQuery { get; set; }

        public object Get(Api.QueryContactGroupBySession request)
        {
            if (request.OrderBy == null)
            {
                request.OrderBy = "Name";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Where(w => w.UserId == this.Session.UserId);
            q.And<ContactGroup>(w => w.TenantId == Session.TenantId);
            return AutoQuery.Execute(request, q);
        }


        public object Get(Api.QueryContactGroupByContact request)
        {
            if (request.OrderBy == null)
            {
                request.OrderBy = "Name";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            q.LeftJoin<ContactGroup, ContactGroupMember>((p, pp) => p.Id == pp.ContactGroupId && pp.ContactId == request.ContactId);
            q.Where(w => w.UserId == this.Session.UserId);
            q.Where<ContactGroup>(w => w.TenantId == Session.TenantId);
            return AutoQuery.Execute(request, q);
        }


        public ContactGroup Get(Api.GetContactGroup request)
        {
            var group = Db.Select(
                                    Db
                                    .From<ContactGroup>()
                                    .Where(c => c.Id == request.Id)
                                  ).SingleOrDefault();
            return group;
        }

        public Api.PostContactGroup Post(Api.PostContactGroup request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var group = (ContactGroup)request;
                    group.TenantId = Session.TenantId;
                    request.Id = (int)Db.Insert(group, true);

                    var permission = new ContactGroupPermission
                    {
                        ContactGroupId = request.Id,
                        UserId = this.Session.UserId
                    };
                    permission.Id = (int)Db.Insert(permission, true);
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

        public Api.PostContactGroup Put(Api.PostContactGroup request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    Db.Update((ContactGroup)request);
                    trx.Commit();
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }

            return request;
        }

        public object Delete(Api.DeleteContactGroup request)
        {
            object model;
            using (var trx = Db.OpenTransaction())
            {
                try
                {

                    var ContactListPermissions = Db.From<ContactGroupPermission>()
                    .Select(c => c.Id).Where(c => c.ContactGroupId == request.Id);
                    Db.Delete(ContactListPermissions);

                    var ContacGroupMembers = Db.From<ContactGroupMember>()
                    .Select(c => c.Id).Where(c => c.ContactGroupId == request.Id);
                    Db.Delete(ContacGroupMembers);

                    model = Db.DeleteById<ContactGroup>(request.Id);
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