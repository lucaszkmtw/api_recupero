using System.Collections.Generic;
using CentralOperativa.Domain.System.Location;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.System.Location
{
    public class Place
    {
        [Route("/system/places/{Id}", "GET")]
        public class Get : IReturn<Domain.System.Location.Place>
        {
            public int? Id { get; set; }
            public string Name { get; set; }
        }

        [Route("/system/places/lookup", "GET")]
        public class Lookup : LookupRequest, IReturn<List<LookupItem>>
        {
            public int? TypeId { get; set; }
        }

        [Route("/system/placenodes", "GET")]
        public class GetNodes{
            public int? Id { get; set; }

            public bool FetchChildren { get; set; }
        }

        public class PlaceNodeResult
        {
            public PlaceNodeResult()
            {
                this.Children = new List<PlaceNodeResult>();
            }

            public int Id { get; set; }

            public int? ParentId { get; set; }

            public string Name { get; set; }

            public int PlaceTypeId { get; set; }

            public string PlaceTypeName { get; set; }

            public int ChildCount {
                get { return this.Children.Count; }
            }

            public List<PlaceNodeResult> Children { get; set; }
        }

        [Route("/system/places", "GET")]
        public class Query : QueryDb<Domain.System.Location.PlaceNode, PlaceQueryResult> , 
            IJoin<Domain.System.Location.PlaceNode, PlaceType>
        {
            public int? ParentId { get; set; }
        }

        public class PlaceQueryResult
        {
            public int Id { get; set; }

            public int? ParentId { get; set; }

            public string ParentName { get; set; }
            
            public string Name { get; set; }

            public int PlaceTypeId { get; set; }

            public string PlaceTypeName { get; set; }

            public int Children { get; set; }
        }

        [Route("/system/places", "POST")]
        [Route("/system/places/{Id}", "Put")]
        public class Post : Domain.System.Location.Place
        { 
        }

        [Route("/system/places/{Id}", "DELETE")]
        public class Delete 
        {
            public int Id { get; set; }
        }
    }
}
