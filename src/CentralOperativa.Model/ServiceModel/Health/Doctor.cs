using System.Collections.Generic;
using ServiceStack;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Infraestructure;

namespace CentralOperativa.ServiceModel.Health
{
    [Route("/health/doctors/{Id}", "GET")]
    public class GetDoctor
    {
        public int Id { get; set; }
    }

    [Route("/health/doctors", "POST")]
    [Route("/health/doctors/{Id}", "PUT")]
    public class PostDoctor : Domain.Health.Doctor
    {
        public int[] SkillIds { get; set; }
    }

    [Route("/health/doctors", "GET")]
    public class QueryDoctors : QueryDb<Domain.Health.Doctor, QueryDoctorsResult>
        , IJoin<Domain.Health.Doctor, Person>
    {
    }

    [Route("/health/doctors/lookup", "GET")]
    public class LookupDoctors : LookupRequest, IReturn<List<LookupItem>>
    {
    }

    public class QueryDoctorsResult
    {
        public int Id { get; set; }
        public string RegistrationNumber { get; set; }
        public string PersonName { get; set; }
    }

    public class Doctor : Domain.Health.Doctor
    {
        public Doctor()
        {
            this.SkillIds = new List<int>();
        }

        public List<int> SkillIds { get; set; }
    }
}