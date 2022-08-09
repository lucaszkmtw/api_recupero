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
    public class OrganismProductService : ApplicationService
    {
        public object Put(Api.PostOrganismProduct request)
        {				
			var q = Db.From<OrganismProduct>()
	             .Where(x => x.OrganismId == request.OrganismId) 
	             .Update(p => p.Status);

			 Db.UpdateOnly(new OrganismProduct { Status = 1 }, onlyFields: q);

			for (var i = 0; i < request.Products.Count; i++) // por cada producto que seleccione
			{
                OrganismProduct vorganismproduct;
				var query = Db.From<OrganismProduct>()
						   .Where(w => w.OrganismId == request.OrganismId
								   && w.ProductId == request.Products[i].Id);

                vorganismproduct = Db.Select(query).SingleOrDefault();

				if (vorganismproduct == null)  // si no existe la relacion => inserta OrganismProduct
                {
                    OrganismProduct orgproduct = new OrganismProduct();
                    orgproduct.OrganismId = request.OrganismId;
                    orgproduct.ProductId = request.Products[i].Id;
                    orgproduct.Status = (int)BusinessPartnerStatus.Active;

                    orgproduct.Id = (int)Db.Insert((OrganismProduct)orgproduct, true);
				}
				else
				{
					if (vorganismproduct.Status == (int)BusinessPartnerStatus.Deleted)
					{
                        vorganismproduct.Status = (int)BusinessPartnerStatus.Active;
						Db.Update((OrganismProduct)vorganismproduct);
					}
				}

			}

			return request;
           }
       
        public object Post(Api.PostOrganismProduct request)
        {
            for (var i = 0; i < request.Products.Count; i++) // por cada producto que seleccione
            {
                OrganismProduct vorganismproduct;
				var query = Db.From<OrganismProduct>()
							.Where(w => w.OrganismId == request.OrganismId // aca hay que poner organismtype
									&& w.ProductId == request.Products[i].Id);

                vorganismproduct = Db.Select(query).SingleOrDefault();
						
				if (vorganismproduct == null)  // si no existe la relacion => inserta OrganismProduct
                {
                    vorganismproduct = new OrganismProduct();
                    vorganismproduct.OrganismId = request.OrganismId;
                    vorganismproduct.ProductId = request.Products[i].Id;
                    vorganismproduct.Status = (int)BusinessPartnerStatus.Active;

                    vorganismproduct.Id = (int)Db.Insert((OrganismProduct)vorganismproduct, true);
				}else{
						if (vorganismproduct.Status == (int)BusinessPartnerStatus.Deleted)
						{
                        vorganismproduct.Status = (int)BusinessPartnerStatus.Active;
							Db.Update((OrganismProduct)vorganismproduct);
						}
					}
				}
			    return request;
		}
        
		public IAutoQueryDb AutoQuery { get; set; }

		public object Any(Api.QueryOrganismProducts request)
		{

			var query = Db.From<OrganismProduct>()
					.Join<OrganismProduct, Product>()
					.Join<OrganismProduct, Domain.System.Persons.Person>()
					.OrderByDescending(q => q.Id)
					.Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));

			return Db.Select(query);
		}

		
		public object Get(Api.GetOrganismProduct request)
		{
			var query = Db.From<OrganismProduct>()
						.Join<OrganismProduct, Product>()
						.Where<OrganismProduct>(o => o.OrganismId == request.Id && o.Status == 0)
						.OrderByDescending(q => q.Id);

			var results = Db.Select(query);

			var model = results.ConvertTo<Api.PostOrganismProduct>();

			model.Id = request.Id;
			model.OrganismId = request.Id;

			model.Products = new List<Product>();

			foreach (var result in results)
			{
				var product = Db.SingleById<Product>(result.ProductId);
                
				model.Products.Add(product);
                
			}

			return model;
            

		}

		public QueryResponse<Api.QueryOrganismProductResult> Get(Api.QueryOrganismProducts request)
		{
			if (request.OrderByDesc == null)
			{
				request.OrderByDesc = "Id";
			}

			var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
			return AutoQuery.Execute(request, q);
		}

		public object Get(Api.LookupOrganismProduct request)
		{
			var query = Db.From<OrganismProduct>()
						.Join<OrganismProduct, Domain.System.Persons.Person>();


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
				Data = Db.Select<Api.GetOrganismProductResult>(query).Select(x => new LookupItem { Id = x.Id, Text = x.PersonName }),
				Total = (int)count
			};
			return result;
		}

    }
}