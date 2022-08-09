using System.Collections.Generic;
using CentralOperativa.Domain.System.Location;
using CentralOperativa.Domain.System.Persons;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System.Location
{
    public class Address
    {
        [Route("/location/address/{Id}", "GET")]

        public class Get : IReturn<Address.GetAddressResult>
        {
            public int? Id { get; set; }
        }

        public class GetAddressResult : Domain.System.Location.Address
        {
            public Domain.System.Location.PlaceNode Place { get; set; }
        }

        [Route("/location/address/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
        }

        [Route("/location/address", "GET")]
        public class Query : QueryDb<Domain.System.Location.Address, QueryResult>, 
            IJoin<PersonAddress, Domain.System.Location.Address>,
            IJoin<PersonAddress, AddressType>,
            IJoin<Domain.System.Location.Address, Domain.System.Location.Place>
        {
        }

        public class QueryResult
        {
            public int Id { get; set; }
            public string Street { get; set; }

            public string StreetNumber { get; set; }

            public string Appartment { get; set; }

            public string Floor { get; set; }

            public int TypeId { get; set; }

            public string TypeName { get; set; }

            public int PlaceId { get; set; }

            public string PlaceName { get; set; }

        }

              
        [Route("/location/address", "POST")]
        [Route("/location/address/{Id}", "PUT")]
        public class Post : Domain.System.Location.Address
        {
            public int PersonId { get; set; }
            public int TypeId { get; set; }

            public string TypeName { get; set; }
        }


        [Route("/location/address/{Id}", "DELETE")]
        public class Delete
        {
            public int Id { get; set; }
        }
    }
}