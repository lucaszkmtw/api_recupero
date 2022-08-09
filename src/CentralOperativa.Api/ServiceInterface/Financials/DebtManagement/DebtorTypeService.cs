using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Domain.Financials.DebtManagement;
using Api = CentralOperativa.ServiceModel.Financials.DebtManagement;
using CentralOperativa.Domain.BusinessPartners;


namespace CentralOperativa.ServiceInterface.Financials.Debtmanagement
{
    [Authenticate]
    public class DebtortypeService : ApplicationService
    {
        public object Put(Api.PostDebtortype request)
        {

			Db.Update((CentralOperativa.Domain.Financials.DebtManagement.DebtorType)request);
			return request;
		}

        public object Post(Api.PostDebtortype request)
        {
			//verifico si el code de lo que esta dando de alta ya existe en la tabla
			//var organismtype = Db.Single<OrganismType>(w => w.Code == request.Code && w.Name == request.Name);
			var DebType = Db.Single<DebtorType>(w => w.BusinessPartnerTypeId == request.BusinessPartnerTypeId);
			if (DebType != null) // existe en la tabla
			{
				DebType.Status = (int)BusinessPartnerStatus.Active;
				DebType.Name = request.Name;
				Db.Update((DebtorType)DebType);
			}
			else
			{ // si no existe.. lo doy de alta
			  request.Id = (int)Db.Insert((CentralOperativa.Domain.Financials.DebtManagement.DebtorType)request, true);
			}
		
            return request;
        }

        public object Delete(Api.DeleteDebtorType request)
        {
            var debtortype = Db.SingleById<DebtorType>(request.Id);
            debtortype.Status = (int)BusinessPartnerStatus.Deleted;
            Db.Update((DebtorType)debtortype);

            return request;
        }

        public IAutoQueryDb AutoQuery { get; set; }
		
       /* public object Any(Api.QueryDebtorTypes request)
        {
			 // tomo solo los deudores activos
            var query = Db.From<Domain.Financials.DebtManagement.DebtorType>()
				.Where (q => q.Status == (int)BusinessPartnerStatus.Active)
                .OrderByDescending(q => q.Name)
                .Limit(request.Skip.GetValueOrDefault(0), request.Take.GetValueOrDefault(10));

            if (!string.IsNullOrEmpty(request.Name))
                query.Where(q => q.Name.Contains(request.Name));

         //   if (!string.IsNullOrEmpty(request.BusinessPartnerTypeId))
           //     query.Where(q => q.BusinessPartnerTypeId.Contains(request.BusinessPartnerTypeId));

            return Db.Select(query);
        }
		*/

		public object Get(Api.GetDebtorType request)
		{
			var model = Db.SingleById<DebtorType>(request.Id);
			return model;
		}



		public QueryResponse<Api.QueryDebtorTypeResult> Get(Api.QueryDebtorTypes request)
		{
			/*if (request.OrderByDesc == null)
			{
				request.OrderByDesc = "Id";
			}*/

			var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
			q.Where<DebtorType>(w => w.Status == (int)BusinessPartnerStatus.Active);
			return AutoQuery.Execute(request, q);
		}



		public LookupResult Get(Api.LookupDebtorType request)
        {
            var query = Db.From<DebtorType>()
                .Select(x => new {x.Id, x.Name});


			if (request.Id.HasValue)
			{
				query.Where(x => x.Id == request.Id.Value);
			}
			else if (request.Ids != null)
			{
				query.Where(x => Sql.In(x.Id, request.Ids));
			}


			//   if (!string.IsNullOrEmpty(request.Q))
			//  {
			//     query = query.Where(q => q.Name.Contains(request.Q) || q.Name.Contains(request.Q));
			//}

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

        
        /*
		public object Delete(Api.DeleteDebtorType request)
		{

			var dot = Db.SingleById<DebtorType>(request.Id);
			dot.Status = (int)BusinessPartnerStatus.Deleted;
			

			Db.Update((DebtorType)dot);

			return request;
		}*/
	}
}