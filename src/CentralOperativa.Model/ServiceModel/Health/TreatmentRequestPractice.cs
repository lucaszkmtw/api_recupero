using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Health
{
    public class TreatmentRequestPractice
    {
        [Route("/health/treatmentrequestpractices/{Id}", "GET")]
        public class Get : IReturn<GetResponse>
        {
            public int Id { get; set; }
        }

        [Route("/health/treatmentrequestpractices", "POST")]
        [Route("/health/treatmentrequestpractices/{Id}", "PUT")]
        public class Post : Domain.Health.TreatmentRequestPractice
        {
        }

        [Route("/health/treatmentrequestpractices", "GET")]
        public class Query : QueryDb<Domain.Health.TreatmentRequestPractice, QueryResult>
        {
        }

        [Route("/health/treatmentrequestpractices/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        public class QueryResult
        {
            public int Id { get; set; }
            public decimal Quantity { get; set; }
            public string Frequency { get; set; }
            public int MedicalPracticeId { get; set; }
            public string MedicalPracticeName { get; set; }
        }

        public class GetResponse : Domain.Health.TreatmentRequestPractice
        {
        }
    }
}