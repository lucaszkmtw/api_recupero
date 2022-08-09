using ServiceStack;
namespace CentralOperativa.ServiceModel.System
{
    [Route("/system/feedbackticket", "POST")]
    public class PostFeedbackTicket
    {
        public FeedbackTicketData Feedback { get; set; }

        public class FeedbackTicketData
        {
            public string Browser { get; set; }
            public string Url { get; set; }
            public string Note { get; set; }
            public string Img { get; set; }
            public string Html { get; set; }
            public int Timestamp { get; set; }
        }
    }
}
