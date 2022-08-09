using ServiceStack;
using ServiceStack.OrmLite;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CentralOperativa.ServiceInterface.System.Workflows
{
    using WorkflowTag = ServiceModel.System.Workflows.WorkflowTag;

    [Authenticate]
    public class WorkflowTagService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public static List<WorkflowTag.WorkflowTagResponse> down;

        public object Get(WorkflowTag.GetWorkflowTag request)
        {
            return Db.SingleById<Domain.System.Workflows.WorkflowTag>(request.Id);
        }

        public object Post(WorkflowTag.Post request)
        {
            var tag = (Domain.System.Workflows.WorkflowTag)request;
            tag.ParentId = request.ParentId != 0 ? request.ParentId : null;

            if (tag.Id == 0)
            {
                request.Id = (int)Db.Insert(tag, true);
            }
            else
            {
                Db.Update(tag, p => p.Id == request.Id);
            }

            return request;
        }

        public object Post(WorkflowTag.UpdateWorkflowTag request)
        {
            Domain.System.Workflows.WorkflowTag objUdapte = Db.SingleById<Domain.System.Workflows.WorkflowTag>(request.Id);
            if (request.ParentId != 0)
                objUdapte.ParentId = request.ParentId;
            else
                objUdapte.ParentId = null;

            Db.Update(objUdapte, p => p.Id == request.Id);

            return true;
        }


        public object Delete(WorkflowTag.DeleteWorkflowTag request)
        {
            try
            {
                var objTag = Db.SingleById<Domain.System.Workflows.WorkflowTag>(request.Id);
                var allItems = Db.Select(Db.From<Domain.System.Workflows.WorkflowTag>());
                ProcessChildren(allItems, objTag);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void ProcessChildren(List<Domain.System.Workflows.WorkflowTag> allItems, Domain.System.Workflows.WorkflowTag item)
        {
            var children = allItems.Where(x => x.ParentId.HasValue && x.ParentId == item.Id).ToList();

            if (children.Any())
                foreach (var child in children)
                    ProcessChildren(allItems, child);

            Db.DeleteById<Domain.System.Workflows.WorkflowTag>(item.Id);
        }


        public object Get(WorkflowTag.QueryWorkflowTag request)
        {
            var allItems = Db.Select(Db.From<Domain.System.Workflows.WorkflowTag>().Where(w => w.WorkflowId == request.WorkflowId));

            var menu = allItems.Where(x => !x.ParentId.HasValue)
                .Select(navigationItem => ProcessNavigationItem(allItems, navigationItem))
                .Where(menuItem => menuItem != null).ToList();

            return menu;
        }

        public object Get(WorkflowTag.LookupWorkflowTags request)
        {
            if (request.ParentId.HasValue)
            {
                var rootItems = Db.Select(Db.From<Domain.System.Workflows.WorkflowTagPath>().Where(w => w.WorkflowId == request.WorkflowId && w.ParentId == request.ParentId)).Select(x => new WorkflowTag.LookupWorkflowTagResponse
                {
                    Id = x.Id,
                    Text = x.Path,
                    HasChildren = x.Children > 0
                });

                return rootItems;
            }
            else
            {
                var rootItems = Db.Select(Db.From<Domain.System.Workflows.WorkflowTagPath>().Where(w => w.WorkflowId == request.WorkflowId && w.ParentId == null)).Select(x => new WorkflowTag.LookupWorkflowTagResponse
                {
                    Id = x.Id,
                    Text = x.Path,
                    HasChildren = x.Children > 0
                });

                return rootItems;
            }
        }

        private static WorkflowTag.WorkflowTagResponse ProcessNavigationItem(List<Domain.System.Workflows.WorkflowTag> allItems, Domain.System.Workflows.WorkflowTag item)
        {
            var children = allItems.Where(x => x.ParentId.HasValue && x.ParentId == item.Id).ToList();
            var menuItem = new WorkflowTag.WorkflowTagResponse
            {
                Id = item.Id,
                ParentId = item.ParentId,
                Name = item.Name,
                WorkflowId = item.WorkflowId
            };

            foreach (var child in children)
            {
                var childMenuItem = ProcessNavigationItem(allItems, child);
                if (childMenuItem != null)
                {
                    menuItem.Items.Add(childMenuItem);
                }
            }

            return menuItem;
        }
    }
}