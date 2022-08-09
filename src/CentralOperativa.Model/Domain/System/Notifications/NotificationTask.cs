using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Notifications
{
    [Alias("NotificationTasks")]
    public class NotificationTask
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(EmailAccount))]
        public int EmailAccountId { get; set; }

        public string Name { get; set; }

        public string MergeTags { get; set; }
    }
}
