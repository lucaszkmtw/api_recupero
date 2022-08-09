using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Health
{
    [Route("/health/medicalpractices/{Id}", "GET")]
    public class GetMedicalPractice : IReturn<GetResponse>
    {
        public int Id { get; set; }
    }

    [Route("/health/medicalpractices", "POST")]
    [Route("/health/medicalpractices/{Id}", "PUT")]
    public class PostMedicalPractice : Domain.Health.MedicalPractice
    {
    }

    [Route("/health/medicalpractices", "GET")]
    public class QueryMedicalPractices : QueryDb<Domain.Health.MedicalPractice, QueryMedicalPracticeResponse>,
        IJoin<Domain.Health.MedicalPractice, Domain.System.Persons.Skill>
    {
    }

    [Route("/health/medicalpractices/lookup", "GET")]
    public class LookupMedicalPractice : LookupRequest, IReturn<List<LookupItem>>
    {
    }

    public class QueryMedicalPracticeResponse
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string SkillName { get; set; }
    }

    public class GetResponse : Domain.Health.MedicalPractice
    {
    }
}