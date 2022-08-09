using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Domain.Catalog;

namespace CentralOperativa.ServiceModel.Catalog
{
    [Route("/catalog/productcategories/{Id}", "GET")]
    public class GetProductCategory
    {
        public int Id { get; set; }
    }

    [Route("/catalog/productcategories", "POST")]
    [Route("/catalog/productcategories/{Id}", "PUT")]
    public class PostProductCategory : ProductCategory
    {
        public List<Product> Products
        {
            get; set;
        }
    }

    [Route("/catalog/productcategories/{Id}", "DELETE")]
    public class DeleteProductCategory : ProductCategory
    {
    }

    [Route("/catalog/productcategories", "GET")]
    public class QueryProductCategories : QueryDb<ProductCategory, QueryProductCategoryResult>
         , IJoin<ProductCategory, Product>
         , IJoin<ProductCategory, Domain.Catalog.Category>
    {

    }

    public class QueryProductCategoryResult : ProductCategory
    {
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public int CategoryStatus { get; set; }
    }

    [Route("/catalog/productcategories/lookup", "GET")]
    public class LookupProductCategory : LookupRequest, IReturn<List<LookupItem>>
    {

    }

    public class GetProductCategoryResult : ProductCategory
    {
        public string PersonName { get; set; }
    }

    [Route("/catalog/productcategories/laws", "GET")]
    public class GetLaws
    {
        public int ProductCategoryId { get; set; }
    }

}
