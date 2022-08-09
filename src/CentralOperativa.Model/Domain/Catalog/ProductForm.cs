using ServiceStack.DataAnnotations;

using CentralOperativa.Domain.Cms.Forms;

namespace CentralOperativa.Domain.Catalog
{
    [Alias("ProductForms"), Schema("catalog")]
    public class ProductForm
    {
        [AutoIncrement]
        public int Id { get; set; }
        [References(typeof(Form))]
        public int FormId { get; set; }
        [References(typeof(Product))]
        public int ProductId { get; set; }
    }
}
