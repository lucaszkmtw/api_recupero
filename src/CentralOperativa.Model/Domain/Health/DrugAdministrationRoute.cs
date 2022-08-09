using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Health
{
    [Alias("DrugAdministrationRoutes")]
    public class DrugAdministrationRoute
    {
        [AutoIncrement]
        public short Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }
    }
}