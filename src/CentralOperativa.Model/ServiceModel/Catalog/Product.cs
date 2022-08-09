using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Catalog
{
    [Route("/catalog/products/{Id}", "GET")]
    public class GetProduct : Domain.Catalog.Product
    {
        public List<Form> Forms { get; set; }

        public class Form
        {
            public int Id { get; set; }
            public int FormId { get; set; }
            public string FormName { get; set; }
        }
    }

    [Route("/catalog/productsconfig/{Id}", "GET")]
    public class GetProductConfig
    {
        public int Id { get; set; }
    }

    [Route("/catalog/productsconfig", "POST")]
    [Route("/catalog/productsconfig/{Id}", "PUT")]
    public class PostProductConfig : Domain.Catalog.ProductConfig
    {
        public string Name { get; set; }
    }

    [Route("/catalog/products", "POST")]
    [Route("/catalog/products/{Id}", "PUT")]
    public class PostProduct : Domain.Catalog.Product
    {
        public PostProduct()
        {
            this.Forms = new List<Form>();
        }

        public List<Form> Forms { get; set; }

        public class Form
        {
            public int? Id { get; set; }

            public int FormId { get; set; }

        }

        public string FieldsJson { get; set; }
        public int ArticleId = 1;
    }
    [Route("/catalog/products", "GET")]
    public class QueryProducts : QueryDb<Domain.Catalog.Product, QueryProductsResult>
    {
    }

    [Route("/catalog/products/lookup", "GET")]
    public class LookupProduct : LookupRequest, IReturn<List<LookupItem>>
    {
        public int? CategoryId { get; set; }
        public int? ByDefault { get; set; }
    }

    [Route("/catalog/products/defaultbycategory", "GET")]
    public class GetDefaultProductByCategory
    {
        public int CategoryId { get; set; }
    }

    public class QueryProductsResult
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Tags { get; set; }
    }

    public class FieldJsonModel
    {
        public int id;
        public string name;
        public string type;
        public List<string> list;
    }
    public class CreditsJSON
    {
        public List<FieldJsonModel> fields;
    }
}
