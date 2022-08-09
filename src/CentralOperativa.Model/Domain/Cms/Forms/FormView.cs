using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.Cms.Forms
{
    [Alias("vForm")]
    public class FormView : Form
    {
        public int Responses { get; set; }
    }
}
