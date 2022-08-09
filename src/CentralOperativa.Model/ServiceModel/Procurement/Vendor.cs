using System;
using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Procurement
{
    public class Vendor
    {
        [Route("/procurement/vendors/{Id}", "GET")]
        public class GetVendor
        {
            public int Id { get; set; }
        }

        public class GetVendorResult : Domain.Procurement.Vendor
        {
            public ServiceModel.BusinessPartners.GetBusinessPartnerResult BusinessPartner { get; set; }
        }

        [Route("/procurement/vendors", "GET")]
        public class QueryVendors : QueryDb<Domain.Procurement.Vendor, QueryVendorsResult>
            , IJoin<Domain.Procurement.Vendor, Domain.BusinessPartners.BusinessPartner>
            , IJoin<Domain.BusinessPartners.BusinessPartner, Domain.System.Persons.Person>
        {
            public string[] Types { get; set; }
        }

        [Route("/procurement/vendors", "POST")]
        [Route("/procurement/vendors/{Id}", "PUT")]
        public class PostVendor : Domain.Procurement.Vendor
        {
            public ServiceModel.BusinessPartners.PostBusinessPartner BusinessPartner { get; set; }
        }

        [Route("/procurement/vendors/{Id}", "DELETE")]
        public class DeleteVendor
        {
            public int Id { get; set; }
        }

        [Route("/procurement/vendors/batch", "POST")]
        public class PostVendorBatch : List<PostVendorBatchItem>
        {
        }

        public class PostVendorBatchItem
        {
            public Domain.System.ImportLog ImportLog { get; set; }
            public PostVendor Vendor { get; set; }
        }

        [Route("/procurement/vendors/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
            public bool? ReturnPersonId { get; set; }
        }

        public class QueryVendorsResult
        {
            public int Id { get; set; }
            public int PersonId { get; set; }
            public string PersonCode { get; set; }
            public string PersonName { get; set; }
        }
    }
}
