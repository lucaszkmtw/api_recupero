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

namespace CentralOperativa.ServiceInterface.Financials.Debtmanagement
{
    [Authenticate]
    public class OrganismCreditTypeService : ApplicationService
    {
        public object Put(Api.PostOrganismCreditType request)
        {				
			var q = Db.From<OrganismCreditType>()
	             .Where(x => x.OrganismId == request.OrganismId) 
	             .Update(p => p.Status);

			 Db.UpdateOnly(new OrganismCreditType { Status = 1 }, onlyFields: q);

			for (var i = 0; i < request.CreditTypes.Count; i++) // por cada tipo de credito que seleccione
			{
				OrganismCreditType vorganismCreditType;
				var query = Db.From<OrganismCreditType>()
						   .Where(w => w.OrganismId == request.OrganismId
								   && w.TypeId == request.CreditTypes[i].Id);

				vorganismCreditType = Db.Select(query).SingleOrDefault();

				if (vorganismCreditType == null)  // si no existe la relacion => inserta OrganismCreditType
				{
					OrganismCreditType orgcredittype = new OrganismCreditType();
					orgcredittype.OrganismId = request.OrganismId;
					orgcredittype.TypeId = request.CreditTypes[i].Id;
					orgcredittype.Status = (int)BusinessPartnerStatus.Active;

					orgcredittype.Id = (int)Db.Insert((OrganismCreditType)orgcredittype, true);
				}
				else
				{
					if (vorganismCreditType.Status == (int)BusinessPartnerStatus.Deleted)
					{
						vorganismCreditType.Status = (int)BusinessPartnerStatus.Active;
						Db.Update((OrganismCreditType)vorganismCreditType);
					}
				}

			}

			return request;
           }
       
        public object Post(Api.PostOrganismCreditType request)
        {
            for (var i = 0; i < request.CreditTypes.Count; i++) // por cada tipo de credito que seleccione
            {
				OrganismCreditType vorganismCreditType;
				var query = Db.From<OrganismCreditType>()
							.Where(w => w.OrganismId == request.OrganismId // aca hay que poner organismtype
									&& w.TypeId == request.CreditTypes[i].Id);

					vorganismCreditType = Db.Select(query).SingleOrDefault();
						
				if (vorganismCreditType == null)  // si no existe la relacion => inserta OrganismCreditType
				{
					vorganismCreditType = new OrganismCreditType();
					vorganismCreditType.OrganismId = request.OrganismId;
					vorganismCreditType.TypeId = request.CreditTypes[i].Id;
					vorganismCreditType.Status = (int)BusinessPartnerStatus.Active;

					vorganismCreditType.Id = (int)Db.Insert((OrganismCreditType)vorganismCreditType, true);
				}else{
						if (vorganismCreditType.Status == (int)BusinessPartnerStatus.Deleted)
						{
							vorganismCreditType.Status = (int)BusinessPartnerStatus.Active;
							Db.Update((OrganismCreditType)vorganismCreditType);
						}
					}
				}
			    return request;
			}
        
		public IAutoQueryDb AutoQuery { get; set; }

		public object Any(Api.QueryOrganismCreditTypes request)
		{

			var query = Db.From<OrganismCreditType>()
					.Join<OrganismCreditType,CreditType>()
					.Join<OrganismCreditType, Domain.System.Persons.Person>()
					.OrderByDescending(q => q.Id)
					.Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));

			return Db.Select(query);
		}

		
		public object Get(Api.GetOrganismCreditType request)
		{
			var query = Db.From<OrganismCreditType>()
						.Join<OrganismCreditType, CreditType>()
						.Where<OrganismCreditType>(o => o.OrganismId == request.Id && o.Status == 0)
						.OrderByDescending(q => q.Id);

			var results = Db.Select(query);

			var model = results.ConvertTo<Api.PostOrganismCreditType>();

			model.Id = request.Id;
			model.OrganismId = request.Id;

			model.CreditTypes = new List<CreditType>();

			foreach (var result in results)
			{
				var creditType = Db.SingleById<CreditType>(result.TypeId);
                
				model.CreditTypes.Add(creditType);
                
			}

			return model;
            

		}

		public QueryResponse<Api.QueryOrganismCreditTypeResult> Get(Api.QueryOrganismCreditTypes request)
		{
			if (request.OrderByDesc == null)
			{
				request.OrderByDesc = "Id";
			}

			var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
			return AutoQuery.Execute(request, q);
		}

		public object Get(Api.LookupOrganismCreditType request)
		{
			var query = Db.From<OrganismCreditType>()
						.Join<OrganismCreditType, Domain.System.Persons.Person>();


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
				Data = Db.Select<Api.GetOrganismCreditTypeResult>(query).Select(x => new LookupItem { Id = x.Id, Text = x.PersonName }),
				Total = (int)count
			};
			return result;
		}

    }
}