using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Notifications
{
    [Alias("EmailTemplates")]
    public class EmailTemplate
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(NotificationTask))]
        public int TaskId { get; set; }

        [References(typeof(User))]
        public int UserId { get; set; }

        public string Name { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }
    }
}
