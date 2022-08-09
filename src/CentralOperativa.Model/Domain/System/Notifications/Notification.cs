using System;
using ServiceStack.DataAnnotations;

namespace CentralOperativa.Domain.System.Notifications
{
    [Alias("Notifications")]
    public class Notification
    {
        [AutoIncrement]
        public int Id { get; set; }

        [References(typeof(EmailTemplate))]
        public int MessageTemplateId { get; set; }

        [References(typeof(User))]
        public int FromUserId { get; set; }

        [References(typeof(User))]
        public int ToUserId { get; set; }

        public DateTime Date { get; set; }

        public string NotificationKey { get; set; }

        public short Opens { get; set; }

        public string State { get; set; }

        public string Comments { get; set; }

        public string MessageId { get; set; }
    }
}
