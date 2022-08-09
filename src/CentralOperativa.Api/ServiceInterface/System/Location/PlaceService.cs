using System.Linq;
using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceModel.System.Location;
using ServiceStack;
using ServiceStack.OrmLite;

namespace CentralOperativa.ServiceInterface.System.Location
{
    public class PlaceService : Service
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public LookupResult Get(Place.Lookup request)
        {
            var query = Db.From<Domain.System.Location.PlaceNode>();

            if (request.Id.HasValue)
            {
                query.Where(w => w.Id == request.Id.Value);
            }
            
            else
            {
                if (!string.IsNullOrEmpty(request.Filter))
                {
                    /*
                    if (request.Filter == "with_no_linked_user")
                    {
                        var userPersonsIds = Db.Column<int>(Db.From<Domain.System.User>().Select(x => x.PersonId));
                        query.And(w => !Sql.In(w.Id, userPersonsIds));
                    }
                    */
                }

                if (!string.IsNullOrEmpty(request.Q))
                {
                    var tokens = request.Q.Split(' ');
                    foreach (var token in tokens)
                    {
                        query.Where(x => x.Name.Contains(token));
                    }
                }

                if (request.TypeId.HasValue)
                {
                    query.Where(w => w.TypeId == request.TypeId);
                }
            }

            var count = Db.Count(query);

            query = query.OrderBy(q => q.Name)
                .Limit(request.PageSize.GetValueOrDefault(100) *  (request.Page.GetValueOrDefault(1) - 1), request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Path }),
                Total = (int)count
            };
            return result;
        }

        public object Get(Place.GetNodes request)
        {
            var model = new List<Place.PlaceNodeResult>();

            if (request.FetchChildren)
            {
                var query = Db.From<Domain.System.Location.PlaceNode>()
                    .Join<Domain.System.Location.PlaceNode, Domain.System.Location.PlaceType>()
                    .Select(x => new { ParentId = x.ParentId ?? 0, x });
                var places = Db.Select<Place.PlaceNodeResult>(query).GroupBy(x => x.ParentId ?? 0)
                .ToDictionary(x => x.Key, x => x.ToList());

                if (places.ContainsKey(request.Id ?? 0))
                {
                    foreach (var place in places[request.Id ?? 0])
                    {
                        model.Add(LoadPlace(place, places));
                    }
                }
            }
            else
            {
                var query = Db.From<Domain.System.Location.PlaceNode>()
                    .Join<Domain.System.Location.PlaceNode, Domain.System.Location.PlaceType>()
                    .Where(w => w.ParentId == request.Id);
                model = Db.Select<Place.PlaceNodeResult>(query);
            }
            return model;
        }

        private Place.PlaceNodeResult LoadPlace(Place.PlaceNodeResult place, Dictionary<int, List<Place.PlaceNodeResult>> places)
        {
            if (places.ContainsKey(place.Id))
            {
                foreach (var item in places[place.Id])
                {
                    place.Children.Add(LoadPlace(item, places));
                }
            }

            return place;
        }

        public QueryResponse<Place.PlaceQueryResult> Get(Place.Query request)
        {
            var query = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            if (request.ParentId.HasValue)
            {
                query.Where(w => w.ParentId == request.ParentId.Value);
            }
            else
            {
                query.Where(w => w.ParentId == null);
            }

            return AutoQuery.Execute(request, query);
        }

        public Domain.System.Location.PlaceNode Get(Place.Get request)
        {
            return Db.SingleById<Domain.System.Location.PlaceNode>(request.Id);
        }


        [Authenticate]
        public Domain.System.Location.Place Post(Place.Post request)
        {
            if (request.ParentId <= 0)
            {
                throw new HttpError(global::System.Net.HttpStatusCode.BadRequest, "400");
            }
            if (request.TypeId <= 0)
            {
                throw new HttpError(global::System.Net.HttpStatusCode.BadRequest, "400");
            }

            var item = request.ConvertTo<Domain.System.Location.Place>();
            item.Id = (int)Db.Insert(item, true);
            return item;
        }

        [Authenticate]
        public object Put(Place.Post request)
        {
            var current = Db.SingleById<Domain.System.Location.Place>(request.Id);
            current.PopulateWith(request);
            Db.Save(current);

            return request.ConvertTo<Domain.System.Location.Place>();
        }

        [Authenticate]
        public object Delete(Place.Delete request)
        {
            return Db.DeleteById<Domain.System.Location.Place>(request.Id);
        }
    }
}