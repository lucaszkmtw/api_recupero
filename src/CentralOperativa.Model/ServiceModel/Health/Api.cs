using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Health
{
    [Route("/health/healthservices/{HealthServiceId}/patients/{PatientId}", "GET")]
    public class GetHealthServicePatient
    {
        public int HealthServiceId { get; set; }
        public int PatientId { get; set; }
    }

    public class GetHealthServicePatientResult : Domain.Health.HealthServicePatient
    {
        public ServiceModel.Health.HealthService.GetResult HealthService { get; set; }
    }

    [Route("/health/healthservices/{HealthServiceId}/patients", "POST")]
    [Route("/health/healthservices/{HealthServiceId}/patients/{PatientId}", "PUT")]
    public class PostHealthServicePatient : Domain.Health.HealthServicePatient
    {
    }

    [Route("/health/healthservices/{HealthServiceId}/patients", "GET")]
    public class QueryHealthServicePatients : QueryDb<Domain.Health.HealthServicePatient, QueryHealthServicePatientsResult>
        , IJoin<Domain.Health.HealthServicePatient, Domain.Health.HealthService>
        , IJoin<Domain.Health.HealthServicePatient, Domain.Health.Patient>
        , IJoin<Domain.Health.Patient, Domain.System.Persons.Person>
    {
        public int HealthServiceId { get; set; }
    }

    [Route("/health/healthservices/{HealthServiceId}/patients/lookup", "GET")]
    public class LookupHealthServicePatient : LookupRequest, IReturn<List<LookupItem>>
    {
    }

    public class QueryHealthServicePatientsResult
    {
        public int Id { get; set; }
        public string PersonName { get; set; }
        public string HealthServicePersonName { get; set; }
        public string CardNumber { get; set; }
        public string ServicePlan { get; set; }
    }
}
