using System.Linq;
using CentralOperativa.Infraestructure;
using ServiceStack;
using ServiceStack.OrmLite;
using Group = CentralOperativa.ServiceModel.System.Persons.Group;

namespace CentralOperativa.ServiceInterface.System.Persons
{
    [Authenticate]
    public class GroupService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Get(Group.LookupGroup request)
        {
            var query = Db.From<Domain.System.Persons.Group>();

            if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where(q => q.Name.Contains(request.Q) || q.Description.Contains(request.Q));
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Name }).ToList(),
                Total = (int)count
            };
            return result;
        }

        public object Get(Group.GetGroup request)
        {

            return Db.SingleById<Domain.System.Persons.Group>(request.Id);
        }
        
        public QueryResponse<Group.QueryGroupResult> Any(Group.QueryGroups request) 
        {
            var query = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, query);
        }
        
        public object Put (Group.PostGroup request)
        {
            var current = Db.SingleById<Domain.System.Persons.Group>(request.Id);
            current.PopulateWith(request);
            Db.Save(current);

            return request.ConvertTo<Domain.System.Persons.Group>();
        }

        public Domain.System.Persons.Group Post(Group.PostGroup request)
        {
            var item = request.ConvertTo<Domain.System.Persons.Group>();
            item.Id = (int)Db.Insert(item, true);
            return item;
        }

        public object Delete(Group.DeleteGroup request)
        {
            return Db.DeleteById<Domain.System.Persons.Group>(request.Id);
        }        
    }
}
