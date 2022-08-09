using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Location
{
    [Alias("AddressTypes")]
    class AddressType
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
