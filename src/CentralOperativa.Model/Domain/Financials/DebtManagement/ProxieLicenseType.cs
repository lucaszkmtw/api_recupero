using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Financials.DebtManagement
{

    [Alias("ProxieLicenses"), Schema("findm")]

    public class ProxieLicenseType
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [References(typeof(LicenseType))]
        public int LicenseTypeId { get; set; }

        [References(typeof(Proxie))]
        public int ProxieId { get; set; }

        public int Status { get; set; }
    }
}
