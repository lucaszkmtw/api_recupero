using ServiceStack.DataAnnotations;
using CentralOperativa.Domain.System.Persons;

namespace CentralOperativa.Domain.Financials.DebtManagement
{
    [Alias("LicensesTypes"), Schema("findm")]
    public class LicenseType
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }

        public int Status { get; set; }

    }

}