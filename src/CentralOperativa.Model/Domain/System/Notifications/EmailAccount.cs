using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Notifications
{
    [Alias("EmailAccounts")]
    public class EmailAccount
    {
        [AutoIncrement]
        public int Id { get; set; }

        public string Name { get; set; }

        public string FromAddress { get; set; }

        public string ReplyTo { get; set; }

        public string Configuration { get; set; }
    }
}
