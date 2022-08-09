using System;
using System.Linq;
using CentralOperativa.Domain.Catalog;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using Contract = CentralOperativa.ServiceModel.Catalog;


namespace CentralOperativa.ServiceInterface.Catalog
{
    [Authenticate]
    public class ProductService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Put(Contract.PostProduct request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    request.TenantId = Session.TenantId;
                    request.Purchase = true;
                    request.Sale = true;

                    Db.Update((Domain.Catalog.Product)request);
                    ProductConfig productConfig = Db.Select(Db.From<ProductConfig>().Where(pc => pc.ProductId == request.Id)).SingleOrDefault();
                    if (productConfig != null)
                    {
                        productConfig.FieldsJSON = request.FieldsJson;
                        Db.Update(productConfig);
                    }

                    save(request);
                    trx.Commit();
                    return request;
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
        }

        public object Post(Contract.PostProduct request)
        {
            using (
                var trx = Db.OpenTransaction())
            {
                try
                {
                    request.TenantId = Session.TenantId;
                    request.Purchase = true;
                    request.Sale = true;
                
                    request.Id = (int)Db.Insert((Product)request, true);

                    if (request.FieldsJson != null)
                    {
                        ProductConfig productConfig = new ProductConfig();
                        productConfig.ProductId = request.Id;
                        productConfig.FieldsJSON = request.FieldsJson;
                        productConfig.Id = (int)Db.Insert(productConfig, true);
                    }

                    save(request);
                    trx.Commit();
                    return request;
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
        }

        private void save(Contract.PostProduct request)
        {

            var productForms = request.Forms.Where(x => x.Id.HasValue);
            if (productForms.Any())
            {
                var productformIds = productForms.Select(x => x.Id.Value).ToList();
                if (productformIds.Any())
                {
                    Db.Delete<ProductForm>(x => x.ProductId == request.Id && !Sql.In(x.Id, productformIds));
                }
                else
                {
                    Db.Delete<ProductForm>(x => x.ProductId == request.Id);
                }
            }

            foreach (var productForm in request.Forms)
            {
                if (productForm.Id.HasValue)
                {
                    Db.Update(new ProductForm
                    {
                        Id = productForm.Id.Value,
                        ProductId = request.Id,
                        FormId = productForm.FormId
                    });
                }
                else
                {
                    Db.Insert(new ProductForm
                    {
                        ProductId = request.Id,
                        FormId = productForm.FormId
                    });
                }
            }
        }

        public object Get(Contract.GetProduct request)
        {
            var product = base.Db.SingleById<Domain.Catalog.Product>(request.Id).ConvertTo<Contract.GetProduct>();

            return product;
        }

        public object Post(Contract.PostProductConfig request)
        {
            if (request.Id == 0)
            {
                var product = Db.SingleById<Product>(request.ProductId);
                if (product != null)
                {
                    var productConfig = Db.Select(Db.From<ProductConfig>().Where(pc => pc.ProductId == product.Id)).SingleOrDefault();
                    if (productConfig == null)
                    {
                        productConfig = new ProductConfig();
                        productConfig.ProductId = product.Id;
                        productConfig.FieldsJSON = request.FieldsJSON;
                        request.Id = (int)Db.Insert(productConfig, true);
                    }
                    else
                    {
                        return HttpError.Conflict("ERR_ProductWithConfig");
                    }
                }
                else
                {
                    return HttpError.Conflict("ERR_NoProduct");
                }
            }
            
            return request;
        }

        public object Put(Contract.PostProductConfig request)
        {
            if (request.ProductId > 0)
            {
                var product = Db.SingleById<Product>(request.ProductId);
                product.Name = request.Name;
                Db.Update(product);
                if (product != null)
                {
                    var productConfig = Db.SingleById<ProductConfig>(request.Id);
                    if (productConfig != null)
                    {
                        productConfig.PopulateWith(request);
                        Db.Update(productConfig);
                    }
                    else
                    {
                        ProductConfig newProductConfig = new ProductConfig();
                        newProductConfig.FieldsJSON = request.FieldsJSON;
                        newProductConfig.ProductId = request.ProductId;
                        newProductConfig.Id = (int)Db.Insert(newProductConfig);
                        //return HttpError.Conflict("ERR_NoProductConfig");
                    }
                }
                else
                {
                    return HttpError.Conflict("ERR_NoProduct");
                }
            }
           
            return request;
        }

        public object Get(Contract.GetProductConfig request)
        {
            var product = Db.SingleById<Product>(request.Id);
            if (product != null)
            {
                var data = Db.Select(Db.From<ProductConfig>().Where(pc => pc.ProductId == request.Id)).SingleOrDefault();
                ProductConfigResult result = new ProductConfigResult();
                result.Name = product.Name;
                result.ProductId = request.Id;
                if (data != null)
                {
                    result.FieldsJSON = data.FieldsJSON;
                    result.Id = data.Id;
                    result.ProductId = data.ProductId;
                }
                return result;
            }
            else
            {
                return HttpError.Conflict("ERR_NoProduct");
            }
        }

        public class ProductConfigResult : ProductConfig
        {
            public String Name { get; set; }
        }
        public class GetDefaultProductByCategoryResult : ProductConfig
        {
            public String ProductName { get; set; }
            public decimal ProductValue { get; set; }
        }

        public object Get(Contract.GetDefaultProductByCategory request)
        {

            var result = Db.Select<GetDefaultProductByCategoryResult>(Db.From<Product>()
                        .Join<Product, ProductCategory>((p, pc) => p.Id == pc.ProductId && pc.CategoryId == request.CategoryId)
                        .Join<ProductCategory, ProductCategoryConfig>((pc, pcc) => pc.Id == pcc.ProductCategoryId && pcc.ByDefault == 1 && pcc.EndDate == null)
                        .Select<Product, ProductCategoryConfig>((pr,prcc) => new
                        {
                           ProductName = pr.Name,
                           pr.Id,
                           ProductValue = prcc.Value
                        }));

            return result;
        }


        public QueryResponse<Contract.QueryProductsResult> Get(Contract.QueryProducts request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public object Get(Contract.LookupProduct request)
        {
            var p = Request.GetRequestParams();

            var query = base.Db.From<Domain.Catalog.Product>();

            if (request.Id.HasValue)
            {
                query.Where(x => x.Id == request.Id);
            }
            if (request.Ids != null)
            {
                query.Where(x => Sql.In(x.Id, request.Ids));
            }
            if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where(q => q.Name.Contains(request.Q)); //|| q.Tags.Contains(request.Q));
            }

            if (request.CategoryId != null && request.CategoryId != default(int))
            {
                var category = Db.SingleById<Domain.Catalog.Category>(request.CategoryId);
                if (category != null)
                {
                    var productsCategories = Db.Select(Db.From<Domain.Catalog.ProductCategory>().Where(pc => pc.CategoryId == category.Id));

                    var productsIds = productsCategories.Select(x => x.ProductId);
                    var productCategoriesIds = productsCategories.Select(x => x.Id);
                    //var productsIds = Db.Select(Db.From<Domain.Catalog.ProductCategory>().Where(pc => pc.CategoryId == category.Id)).Select(x => x.ProductId).ToList();
                    query.Where(q => Sql.In(q.Id, productsIds));
                }
            }


            //if (p.ContainsKey("organismId"))
            //{
            //    var organism = Db.SingleById<Domain.Financials.DebtManagement.Organism>(p["organismId"]);
            //    if (organism != null)
            //    {
            //        var productsIds = Db.Select(Db.From<Domain.Financials.DebtManagement.OrganismProduct>().Where(op => op.OrganismId == organism.Id)).Select(x => x.ProductId).ToList();
            //        query.Where(q => Sql.In(q.Id, productsIds));
            //    }
            //}
            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Name }),
                Total = (int)count
            };
            return result;
        }
    }
}