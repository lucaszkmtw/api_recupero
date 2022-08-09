using System.Collections.Generic;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System.Notifications
{
    public class Notification
    {
        [Route("/system/notifications/test")]
        public class GetTest
        {
        }

        [Route("/system/notifications/send", "POST")]
        public class PostSendNotification
        {
            public string FromName { get; set; }

            public string FromAddress { get; set; }

            public List<string> ToAddresses { get; set; }

            public List<string> BccAddresses { get; set; }

            public string Subject { get; set; }

            public string Body { get; set; }
        }
    }
}
