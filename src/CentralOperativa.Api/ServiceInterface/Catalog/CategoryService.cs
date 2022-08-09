using System;
using System.Collections.Generic;
using System.Linq;
using ServiceStack;
using ServiceStack.OrmLite;
using Contract = CentralOperativa.ServiceModel.Catalog;


using CentralOperativa.Domain.Catalog;
using CentralOperativa.Infraestructure;



namespace CentralOperativa.ServiceInterface.Catalog
{
    using Api = ServiceModel.Catalog.Category;

    [Authenticate]
    public class CategoryService : ApplicationService
    {
        public object Get(Api.GetCategory request)
        {
            return Db.SingleById<Domain.Catalog.Category>(request.Id);
        }

        public object Get(Api.QueryCategories request)
        {
            var data = Db.Select(Db.From<Domain.Catalog.Category>().Where(w => w.TenantId == Session.TenantId)).ToList();
            var model = new List<Api.GetCategoryResponse>();
            foreach(var item in data.Where(x => x.ParentId == request.ParentId))
            {
                model.Add(ProcessNavigationItem(data, item));
            }

            return model;
        }

        public object Post(Api.PostCategory request)
        {
            var data = (Domain.Catalog.Category)request;
            if (data.Id == 0)
            {
                request.TenantId = Session.TenantId;
                request.Id = (int)Db.Insert(data, true);
            }
            else
            {
                request.TenantId = Session.TenantId;
                Db.Update(data, p => p.Id == request.Id);
            }

            return request;
        }

        public object Delete(Api.DeleteCategory request)
        {
            try
            {
                Db.DeleteById<Domain.Catalog.Category>(request.Id);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static Api.GetCategoryResponse ProcessNavigationItem(List<Domain.Catalog.Category> allItems, Domain.Catalog.Category item)
        {
            var children = allItems.Where(x => x.ParentId.HasValue && x.ParentId == item.Id).ToList();
            var model = new Api.GetCategoryResponse
            {
                Id = item.Id,
                ParentId = item.ParentId,
                Name = item.Name,
            };

            foreach (var child in children)
            {
                var childMenuItem = ProcessNavigationItem(allItems, child);
                if (childMenuItem != null)
                {
                    model.Items.Add(childMenuItem);
                }
            }

            return model;
        }

        public LookupResult Get(Api.LookupCategory request)
        {
            var query = Db.From<Category>()
                .Select(x => new { x.Id, x.Name });
            
            if (request.Id.HasValue)
            {
                query.Where(x => x.Id == request.Id.Value);
            }
            else if (request.Ids != null)
            {
                query.Where(x => Sql.In(x.Id, request.Ids));
            }

            if (request.OrganismId !=  default(int))
            {
                var categoriesIds = Db.Select(Db.From<Domain.Financials.DebtManagement.OrganismCategory>().Where(oc => oc.OrganismId == request.OrganismId)).Select(x => x.CategoryId).ToList();
                if (categoriesIds != null)
                {
                    query.Where(x => Sql.In(x.Id, categoriesIds));
                }
            }


            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
               .Limit((request.Page.GetValueOrDefault(1) - 1) * request.PageSize.GetValueOrDefault(100), request.PageSize.GetValueOrDefault(100));

            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Name }),
                Total = (int)count
            };
            return result;
        }

    }
}