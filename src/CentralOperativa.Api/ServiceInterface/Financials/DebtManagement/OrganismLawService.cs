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
    public class OrganismLawService : ApplicationService
    {
        public object Put(Api.PostOrganismLaw request)
        {				
			var q = Db.From<OrganismLaw>()
	             .Where(x => x.OrganismId == request.OrganismId) 
	             .Update(p => p.Status);

			 Db.UpdateOnly(new OrganismLaw { Status = 1 }, onlyFields: q);

			for (var i = 0; i < request.Laws.Count; i++) // por cada Law que seleccione
            {
                OrganismLaw vorganismlaw;
				var query = Db.From<OrganismLaw>()
						   .Where(w => w.OrganismId == request.OrganismId
								   && w.LawId == request.Laws[i].Id);

                vorganismlaw = Db.Select(query).SingleOrDefault();

				if (vorganismlaw == null)  // si no existe la relacion => inserta OrganismLaw
                {
                    OrganismLaw orglaw = new OrganismLaw();
                    orglaw.OrganismId = request.OrganismId;
                    orglaw.LawId = request.Laws[i].Id;
                    orglaw.Status = (int)BusinessPartnerStatus.Active;

                    orglaw.Id = (int)Db.Insert((OrganismLaw)orglaw, true);
				}
				else
				{
					if (vorganismlaw.Status == (int)BusinessPartnerStatus.Deleted)
					{
                        vorganismlaw.Status = (int)BusinessPartnerStatus.Active;
						Db.Update((OrganismLaw)vorganismlaw);
					}
				}

			}

			return request;
           }
       
        public object Post(Api.PostOrganismLaw request)
        {
            for (var i = 0; i < request.Laws.Count; i++) // por cada Law que seleccione
            {
                OrganismLaw vorganismlaw;
				var query = Db.From<OrganismLaw>()
							.Where(w => w.OrganismId == request.OrganismId // aca hay que poner organismtype
									&& w.LawId == request.Laws[i].Id);

                vorganismlaw = Db.Select(query).SingleOrDefault();
						
				if (vorganismlaw == null)  // si no existe la relacion => inserta OrganismProduct
                {
                    vorganismlaw = new OrganismLaw();
                    vorganismlaw.OrganismId = request.OrganismId;
                    vorganismlaw.LawId = request.Laws[i].Id;
                    vorganismlaw.Status = (int)BusinessPartnerStatus.Active;

                    vorganismlaw.Id = (int)Db.Insert((OrganismLaw)vorganismlaw, true);
				}else{
						if (vorganismlaw.Status == (int)BusinessPartnerStatus.Deleted)
						{
                        vorganismlaw.Status = (int)BusinessPartnerStatus.Active;
							Db.Update((OrganismLaw)vorganismlaw);
						}
					}
				}
			    return request;
		}
        
		public IAutoQueryDb AutoQuery { get; set; }

		public object Any(Api.QueryOrganismLaws request)
		{

			var query = Db.From<OrganismLaw>()
					.Join<OrganismLaw, Law>()
					.Join<OrganismLaw, Domain.System.Persons.Person>()
					.OrderByDescending(q => q.Id)
					.Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));

			return Db.Select(query);
		}

		
		public object Get(Api.GetOrganismLaw request)
		{
			var query = Db.From<OrganismLaw>()
						.Join<OrganismLaw, Law>()
						.Where<OrganismLaw>(o => o.OrganismId == request.Id && o.Status == 0)
						.OrderByDescending(q => q.Id);

			var results = Db.Select(query);

			var model = results.ConvertTo<Api.PostOrganismLaw>();

			model.Id = request.Id;
			model.OrganismId = request.Id;

			model.Laws = new List<Law>();

			foreach (var result in results)
			{
				var product = Db.SingleById<Law>(result.LawId);
                
				model.Laws.Add(product);
                
			}

			return model;
            

		}

		public QueryResponse<Api.QueryOrganismLawResult> Get(Api.QueryOrganismLaws request)
		{
			if (request.OrderByDesc == null)
			{
				request.OrderByDesc = "Id";
			}

			var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
			return AutoQuery.Execute(request, q);
		}

		public object Get(Api.LookupOrganismLaw request)
		{
			var query = Db.From<OrganismLaw>()
						.Join<OrganismLaw, Domain.System.Persons.Person>();


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
				Data = Db.Select<Api.GetOrganismLawResult>(query).Select(x => new LookupItem { Id = x.Id, Text = x.PersonName }),
				Total = (int)count
			};
			return result;
		}

    }
}