using System.Linq;
using CentralOperativa.Domain.System.Persons;
using ServiceStack;
using ServiceStack.OrmLite;
using CentralOperativa.Infraestructure;

using Api = CentralOperativa.ServiceModel.Health;

namespace CentralOperativa.ServiceInterface.Health
{
    [Authenticate]
    public class DoctorService : ApplicationService
    {
        public IAutoQueryDb AutoQuery { get; set; }

        public object Put(Api.PostDoctor request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    var currentSkills = Db.Select(Db.From<PersonSkill>().Where(w => w.PersonId == request.PersonId));
                    Db.Delete<PersonSkill>(x => x.PersonId == request.PersonId && !Sql.In(x.SkillId, request.SkillIds));
                    foreach (var skillId in request.SkillIds.Except(currentSkills.Select(x => x.SkillId)))
                    {
                        Db.Insert(new PersonSkill {PersonId = request.PersonId, SkillId = skillId});
                    }

                    Db.Update((Domain.Health.Doctor) request);

                    trx.Commit();

                    return request;
                }
                catch
                {
                    trx.Rollback();
                    return null;
                }
            }
        }

        public object Post(Api.PostDoctor request)
        {
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    request.Id = (int)Db.Insert((Domain.Health.Doctor)request, true);
                    foreach (var skillId in request.SkillIds)
                    {
                        Db.Insert(new PersonSkill { PersonId = request.PersonId, SkillId = skillId });
                    }

                    trx.Commit();

                    return request;
                }
                catch
                {
                    trx.Rollback();
                    return null;
                }
            }
        }

        public object Get(Api.GetDoctor request)
        {
            var doctor = Db.SingleById<Domain.Health.Doctor>(request.Id);
            var model = doctor.ConvertTo<Api.Doctor>();
            var skills = Db.Column<int>(Db.From<Domain.System.Persons.PersonSkill>()
                .Where(w => w.PersonId == doctor.PersonId)
                .Select(x => x.SkillId));
            model.SkillIds.AddRange(skills);
            return model;
        }

        public QueryResponse<Api.QueryDoctorsResult> Get(Api.QueryDoctors request)
        {
            if (request.OrderByDesc == null)
            {
                request.OrderByDesc = "Id";
            }

            var q = AutoQuery.CreateQuery(request, Request.GetRequestParams());
            return AutoQuery.Execute(request, q);
        }

        public object Get(Api.LookupDoctors request)
        {
            var query = Db.From<Domain.Health.Doctor>()
                .Join<Domain.Health.Doctor, Person>();

            if (request.Id.HasValue)
            {
                query.Where(x => x.Id == request.Id.Value);
            }
            else if (request.Ids != null)
            {
                query.Where(x => Sql.In(x.Id, request.Ids));
            }
            if (!string.IsNullOrEmpty(request.Q))
            {
                query.Where(q => q.RegistrationNumber.Contains(request.Q));
                query.Or<Person>(q => q.Name.Contains(request.Q));
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select<Api.QueryDoctorsResult>(query).Select(x => new LookupItem { Id = x.Id, Text = x.PersonName + " (" + x.RegistrationNumber + ")" }),
                Total = (int)count
            };
            return result;
        }
    }
}
