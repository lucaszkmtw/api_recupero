using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.OrmLite;

using CentralOperativa.Domain.BusinessPartners;
using CentralOperativa.Infraestructure;

	//using CentralOperativa.ServiceInterface.System.Persons;


namespace CentralOperativa.ServiceInterface.BusinessPartnerType
{
    [Authenticate]
    public class BusinessPartnerTypeService : ApplicationService
    {

		public object Get(CentralOperativa.Domain.BusinessPartners.BusinessPartnerType request)
		{
			var model = Db.SingleById<CentralOperativa.Domain.BusinessPartners.BusinessPartnerType>(request.Id);
			return model;
		}

		 
		public LookupResult Get (CentralOperativa.ServiceModel.BusinessPartners.LookupBusinessPartnerType request)
		{
			var query = Db.From<CentralOperativa.Domain.BusinessPartners.BusinessPartnerType>()
				.Select(x => new { x.Id, x.Name });
			
            //if (request.Id.HasValue)
			   if (request.Id.HasValue)
			{	
				query.Where(x => x.Id == request.Id.Value);
			}
			else if (request.Ids != null)
			{
				query.Where(x => Sql.In(x.Id, request.Ids));
			}
			
			var count = Db.Count(query);

			query = query.OrderByDescending(q => q.Id)
			   .Limit((request.Page.GetValueOrDefault(1) - 1) * request.PageSize.GetValueOrDefault(100), request.PageSize.GetValueOrDefault(100));
			

			var result = new LookupResult
			{
				Data = Db.Select(query).Select(x => new LookupItem {Id=x.Id,  Text = x.Name}),
				Total = (int)count
			};
			return result;
		}

	}
}