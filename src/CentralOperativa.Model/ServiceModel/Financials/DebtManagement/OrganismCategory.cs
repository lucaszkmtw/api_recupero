using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Domain.Financials.DebtManagement;
using CentralOperativa.Domain.Catalog;

namespace CentralOperativa.ServiceModel.Financials.DebtManagement
{
    [Route("/financials/debtmanagement/organismcategories/{Id}", "GET")]
    public class GetOrganismCategory
    {
        public int Id { get; set; }
    }

    [Route("/financials/debtmanagement/organismcategories", "POST")]
    [Route("/financials/debtmanagement/organismcategories/{Id}", "PUT")]
    public class PostOrganismCategory : OrganismCategory
    {
        public List<Category> Categories
        {
            get; set;
        }
    }

    [Route("/financials/debtmanagement/organismcategories/{Id}", "DELETE")]
    public class DeleteOrganismCategory : OrganismCategory
    {
    }

    [Route("/financials/debtmanagement/organismcategories", "GET")]
    public class QueryOrganismCategories : QueryDb<OrganismCategory, QueryOrganismCategoryResult>
        , IJoin<OrganismCategory, Category>
        , IJoin<OrganismCategory, Organism>
        , IJoin<Organism, Person>
    {

    }

    public class QueryOrganismCategoryResult
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string PersonName { get; set; }
        public int OrganismId { get; set; }
        public int OrganismStatus { get; set; }
        public int OrganismProductStatus { get; set; }
    }

    [Route("/financials/debtmanagement/organismcategories/lookup", "GET")]
    public class LookupOrganismCategory : LookupRequest, IReturn<List<LookupItem>>
    {

    }

    public class GetOrganismCategoryResult : OrganismCategory
    {
        public string PersonName { get; set; }
    }

    public class OrganismCategory : Domain.Financials.DebtManagement.OrganismCategory
    {
        public OrganismCategory()
        {
        }
    }


}