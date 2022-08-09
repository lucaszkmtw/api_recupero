using System;
using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;
using CentralOperativa.Domain.Financials.DebtManagement;

namespace CentralOperativa.ServiceModel.Financials.DebtManagement
{
    [Route("/financials/debtmanagement/prescriptions/{Id}", "GET")]
    public class GetPrescription
    {
        public int Id { get; set; }
    }

    [Route("/financials/debtmanagement/prescriptions", "POST")]
    [Route("/financials/debtmanagement/prescriptions/{Id}", "PUT")]
    public class PostPrescription : Domain.Financials.DebtManagement.Prescription
    {
    }

    [Route("/financials/debtmanagement/prescriptions/{Id}", "DELETE")]
    public class DeletePrescription : Domain.Financials.DebtManagement.Prescription
    {
    }

    [Route("/financials/debtmanagement/prescriptions", "GET")]
    public class QueryPrescriptions : QueryDb<Domain.Financials.DebtManagement.Prescription, QueryPrescriptionResult>
        , IJoin<Domain.Financials.DebtManagement.Prescription, Normative>        
    {
    }

    public class QueryPrescriptionResult 
    {
        public int Id { get; set; }
        public int NormativeId { get; set; }        
        public string NormativeName { get; set; }
        public int NumberOfDays { get; set; }
        public string Observations { get; set; }
        public string Suspends { get; set; }
        public string Interrupt { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int Status { get; set; }
    }

    [Route("/financials/debtmanagement/prescriptions/lookup", "GET")]
    public class LookupPrescription : LookupRequest, IReturn<List<LookupItem>>
    {     
    }
    /*
    public class GetPrescriptionResult : Prescription
    {
        public string NormativeName { get; set; }
        public string NumberOfdays { get; set; }
    }
    */
    public class Prescription : Domain.Financials.DebtManagement.Prescription
    {
        public Prescription()
        {
        }
    }


}
