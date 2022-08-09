using System.Linq;
using CentralOperativa.Infraestructure;
using CentralOperativa.Domain.System.Persons;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Domain.Financials.DebtManagement;
using Api = CentralOperativa.ServiceModel.Financials.DebtManagement;
using CentralOperativa.ServiceModel.System.Persons;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using CentralOperativa.Domain.BusinessPartners;
using CentralOperativa.Domain.Catalog;

namespace CentralOperativa.ServiceInterface.Financials.Debtmanagement
{
    [Authenticate]
    public class OrganismCategoryService : ApplicationService
    {
        public object Put(Api.PostOrganismCategory request)
        {				
			var q = Db.From<OrganismCategory>()
	             .Where(x => x.OrganismId == request.OrganismId) 
	             .Update(p => p.Status);

			 Db.UpdateOnly(new OrganismCategory { Status = 1 }, onlyFields: q);

			for (var i = 0; i < request.Categories.Count; i++) // por cada Category que seleccione
            {
                OrganismCategory vorganismcategory;
				var query = Db.From<OrganismCategory>()
						   .Where(w => w.OrganismId == request.OrganismId
								   && w.CategoryId == request.Categories[i].Id);

                vorganismcategory = Db.Select(query).SingleOrDefault();

				if (vorganismcategory == null)  // si no existe la relacion => inserta OrganismProduct
                {
                    OrganismCategory orgcategory = new OrganismCategory();
                    orgcategory.OrganismId = request.OrganismId;
                    orgcategory.CategoryId = request.Categories[i].Id;
                    orgcategory.Status = (int)BusinessPartnerStatus.Active;

                    orgcategory.Id = (int)Db.Insert((OrganismCategory)orgcategory, true);
				}
				else
				{
					if (vorganismcategory.Status == (int)BusinessPartnerStatus.Deleted)
					{
                        vorganismcategory.Status = (int)BusinessPartnerStatus.Active;
						Db.Update((OrganismCategory)vorganismcategory);
					}
				}

			}

			return request;
           }
       
        public object Post(Api.PostOrganismCategory request)
        {
            for (var i = 0; i < request.Categories.Count; i++) // por cada Category que seleccione
            {
                OrganismCategory vorganismcategory;
				var query = Db.From<OrganismCategory>()
							.Where(w => w.OrganismId == request.OrganismId // aca hay que poner organismtype
									&& w.CategoryId == request.Categories[i].Id);

                vorganismcategory = Db.Select(query).SingleOrDefault();
						
				if (vorganismcategory == null)  // si no existe la relacion => inserta OrganismProduct
                {
                    vorganismcategory = new OrganismCategory();
                    vorganismcategory.OrganismId = request.OrganismId;
                    vorganismcategory.CategoryId = request.Categories[i].Id;
                    vorganismcategory.Status = (int)BusinessPartnerStatus.Active;

                    vorganismcategory.Id = (int)Db.Insert((OrganismCategory)vorganismcategory, true);
				}else{
						if (vorganismcategory.Status == (int)BusinessPartnerStatus.Deleted)
						{
                        vorganismcategory.Status = (int)BusinessPartnerStatus.Active;
							Db.Update((OrganismCategory)vorganismcategory);
						}
					}
				}
			    return request;
		}
        
		public IAutoQueryDb AutoQuery { get; set; }

		public object Any(Api.QueryOrganismCategories request)
		{

            var query = Db.From<OrganismCategory>()
                    .Join<OrganismCategory, Category>()
                    .Join<OrganismCategory, Domain.System.Persons.Person>()
                    .OrderByDescending(q => q.Id)
					.Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));

			return Db.Select(query);
		}

		
		public object Get(Api.GetOrganismCategory request)
		{
			var query = Db.From<OrganismCategory>()
						.Join<OrganismCategory, Category>()
						.Where<OrganismCategory>(o => o.OrganismId == request.Id && o.Status == 0)
						.OrderByDescending(q => q.Id);

			var results = Db.Select(query);

			var model = results.ConvertTo<Api.PostOrganismCategory>();

			model.Id = request.Id;
			model.OrganismId = request.Id;

			model.Categories = new List<Category>();

			foreach (var result in results)
			{
				var category = Db.SingleById<Category>(result.CategoryId);
                
				model.Categories.Add(category);
                
			}

			return model;
            

		}

		public QueryResponse<Api.QueryOrganismCategoryResult> Get(Api.QueryOrganismCategories request)
		{
			if (request.OrderByDesc == null)
			{
				request.OrderByDesc = "Id";
			}

			var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
			return AutoQuery.Execute(request, q);
		}

		public object Get(Api.LookupOrganismCategory request)
		{
			var query = Db.From<OrganismCategory>()
						.Join<OrganismCategory, Domain.System.Persons.Person>();


			if (request.Id.HasValue)
			{
				query.Where(x => x.Id == request.Id.Value);
			}
			else if (request.Ids != null)
			{
				query.Where(x => Sql.In(x.Id, request.Ids));
			}

			//query.Where(x => x.Status == (int)BusinessPartnerStatus.Active);

			var count = Db.Count(query);

			query = query.OrderByDescending(q => q.Id)
				.Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


			var result = new LookupResult
			{
				Data = Db.Select<Api.GetOrganismCategoryResult>(query).Select(x => new LookupItem { Id = x.Id, Text = x.PersonName }),
				Total = (int)count
			};
			return result;
		}

    }
}