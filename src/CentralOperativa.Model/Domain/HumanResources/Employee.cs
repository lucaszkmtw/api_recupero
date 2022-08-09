using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.HumanResources
{
    [Alias("RRHH_recurso")]
    public class Employee
    {
        [AutoIncrement]
        public int Id { get; set; }

        [Alias("nombre")]
        public string FirstName { get; set; }

        [Alias("apellido")]
        public string LastName { get; set; }

        [Alias("cuil")]
        public string VatNumber { get; set; }

        [Alias("activo")]
        public bool IsActive { get; set; }

        [Ignore]
        public string FullName
        {
            get { return this.LastName + ", " + this.FirstName; }
        }
    }
}