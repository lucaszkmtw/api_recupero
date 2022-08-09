using System.Linq;
using CentralOperativa.Infraestructure;
using CentralOperativa.Domain.System.Persons;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Domain.Financials.DebtManagement;
using Api = CentralOperativa.ServiceModel.Catalog;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using CentralOperativa.Domain.BusinessPartners;
using CentralOperativa.Domain.Catalog;

namespace CentralOperativa.ServiceInterface.Catalog
{
    [Authenticate]
    public class ProductCategoryService : ApplicationService
    {
        public object Put(Api.PostProductCategory request)
        {				
			var q = Db.From<ProductCategory>()
	             .Where(x => x.CategoryId == request.CategoryId) 
	             .Update(p => p.Status);

			 Db.UpdateOnly(new ProductCategory { Status = 1 }, onlyFields: q);

			for (var i = 0; i < request.Products.Count; i++) // por cada producto que seleccione
			{
                ProductCategory vproductcategory;
				var query = Db.From<ProductCategory>()
						   .Where(w => w.CategoryId == request.CategoryId
                                   && w.ProductId == request.Products[i].Id);

                vproductcategory = Db.Select(query).SingleOrDefault();

				if (vproductcategory == null)  // si no existe la relacion => inserta OrganismProduct
                {
                    ProductCategory productcategory = new ProductCategory();
                    productcategory.CategoryId = request.CategoryId;
                    productcategory.ProductId = request.Products[i].Id;
                    productcategory.Status = (int)BusinessPartnerStatus.Active;

                    productcategory.Id = (int)Db.Insert((ProductCategory)productcategory, true);
				}
				else
				{
					if (vproductcategory.Status == (int)BusinessPartnerStatus.Deleted)
					{
                        vproductcategory.Status = (int)BusinessPartnerStatus.Active;
						Db.Update((ProductCategory)vproductcategory);
					}
				}

			}

			return request;
           }
       
        public object Post(Api.PostProductCategory request)
        {
            for (var i = 0; i < request.Products.Count; i++) // por cada producto que seleccione
            {
                ProductCategory vproductcategory;
				var query = Db.From<ProductCategory>()
							.Where(w => w.CategoryId == request.CategoryId // aca hay que poner organismtype
                                    && w.ProductId == request.Products[i].Id);

                vproductcategory = Db.Select(query).SingleOrDefault();
						
				if (vproductcategory == null)  // si no existe la relacion => inserta OrganismProduct
                {
                    vproductcategory = new ProductCategory();
                    vproductcategory.CategoryId = request.CategoryId;
                    vproductcategory.ProductId = request.Products[i].Id;
                    vproductcategory.Status = (int)BusinessPartnerStatus.Active;

                    vproductcategory.Id = (int)Db.Insert((ProductCategory)vproductcategory, true);
				}else{
						if (vproductcategory.Status == (int)BusinessPartnerStatus.Deleted)
						{
                        vproductcategory.Status = (int)BusinessPartnerStatus.Active;
							Db.Update((ProductCategory)vproductcategory);
						}
					}
				}
			    return request;
		}

       
        public IAutoQueryDb AutoQuery { get; set; }

		public object Any(Api.QueryProductCategories request)
		{

			var query = Db.From<ProductCategory>()
					.Join<ProductCategory, Product>()
					.Join<ProductCategory, Category>()
					.OrderByDescending(q => q.Id)
					.Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));

			return Db.Select(query);
		}

		
		public object Get(Api.GetProductCategory request)
		{
			var query = Db.From<ProductCategory>()
						.Join<ProductCategory, Product>()
						.Where<ProductCategory>(o => o.CategoryId == request.Id && o.Status == 0)
						.OrderByDescending(q => q.Id);

			var results = Db.Select(query);

			var model = results.ConvertTo<Api.PostProductCategory>();

			model.Id = request.Id;
			model.CategoryId = request.Id;

			model.Products = new List<Product>();

			foreach (var result in results)
			{
				var product = Db.SingleById<Product>(result.ProductId);
                
				model.Products.Add(product);
                
			}

			return model;
            

		}

		public QueryResponse<Api.QueryProductCategoryResult> Get(Api.QueryProductCategories request)
		{
			if (request.OrderByDesc == null)
			{
				request.OrderByDesc = "Id";
			}

			var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
			return AutoQuery.Execute(request, q);
		}

		public object Get(Api.LookupProductCategory request)
		{
			var query = Db.From<OrganismProduct>()
						.Join<OrganismProduct, Category>();


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
				Data = Db.Select<Api.GetProductCategoryResult>(query).Select(x => new LookupItem { Id = x.Id, Text = x.PersonName }),
				Total = (int)count
			};
			return result;
		}

        public object Get(Api.GetLaws request)
        {

            var result = Db.Select(Db.From<Law>()
                        .Join<Law, ProductCategoryLaw>((l, pcl) => l.Id == pcl.LawId && pcl.ProductCategoryId == request.ProductCategoryId));

            return result;
        }
    }
}