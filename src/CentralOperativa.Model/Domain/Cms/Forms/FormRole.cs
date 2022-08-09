using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Cms.Forms
{
    [Alias("FormRoles")]
    public class FormRole
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(Form))]
        public int FormId { get; set; }

        [References(typeof(Domain.System.Role))]
        public int RoleId { get; set; }

        public int Quota { get; set; }
    }
}