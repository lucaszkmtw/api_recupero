using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Domain.Financials.DebtManagement;
using CentralOperativa.Domain.Catalog;

namespace CentralOperativa.ServiceModel.Financials.DebtManagement
{
    [Route("/financials/debtmanagement/organismproducts/{Id}", "GET")]
    public class GetOrganismProduct
    {
        public int Id { get; set; }
    }

    [Route("/financials/debtmanagement/organismproducts", "POST")]
    [Route("/financials/debtmanagement/organismproducts/{Id}", "PUT")]
    public class PostOrganismProduct : OrganismProduct
    {
        public List<Product> Products
        {
			get; set; }
    }

    [Route("/financials/debtmanagement/organismproducts/{Id}", "DELETE")]
    public class DeleteOrganismProduct : OrganismProduct
    {
    }

    [Route("/financials/debtmanagement/organismproducts", "GET")]
    public class QueryOrganismProducts : QueryDb<OrganismProduct, QueryOrganismProductResult>
        , IJoin<OrganismProduct, Product>
        , IJoin<OrganismProduct, Organism>
		, IJoin<Organism, Person>
	{

    }

    public class QueryOrganismProductResult
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string PersonName { get; set; }
		public int OrganismId { get; set; }
		public int OrganismStatus { get; set; }
		public int OrganismProductStatus { get; set; }
	}

    [Route("/financials/debtmanagement/organismproducts/lookup", "GET")]
    public class LookupOrganismProduct : LookupRequest, IReturn<List<LookupItem>>
    {
        
    }

    public class GetOrganismProductResult : OrganismProduct
    {
        public string PersonName { get; set; }
    }

    public class OrganismProduct : Domain.Financials.DebtManagement.OrganismProduct
    {
        public OrganismProduct()
        {
        }
    }


}
