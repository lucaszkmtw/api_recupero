using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Sales
{
    [Alias("Clients")]
    public class Client
    {
        [PrimaryKey]
        public int Id { get; set; }
    }
}