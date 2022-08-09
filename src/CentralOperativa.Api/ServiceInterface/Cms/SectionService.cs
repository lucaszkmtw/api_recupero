using System;
using System.Linq;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceModel.Cms;
using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;

namespace CentralOperativa.ServiceInterface.Cms
{
    [Authenticate]
    public class SectionService : ApplicationService
    {
        public object Get(GetSection request)
        {
            var sectionContentService = this.TryResolve<SectionContentService>();
            GetSectionResponse model = null;
            int total;
            var query = Db
                .From<Domain.Cms.Section>()
                .Where(w => w.TenantId == Session.TenantId)
                .And(w => w.Id == request.Id);

            var section = Db.Select(query).FirstOrDefault();
            if (section != null)
            {
                model = section.ConvertTo<GetSectionResponse>();
                if (request.IncludeSubSections)
                {
                    model.Contents = sectionContentService.GetPagesBySection(section, true, false, int.MaxValue, 0,
                        out total);

                    model.Sections = Db.Select<GetSectionResponse>(
                        Db.From<Domain.Cms.Section>()
                            .Where(w => w.TenantId == Session.TenantId && w.ParentId == model.Id)
                            .OrderBy(o => o.ListIndex)
                            .ThenBy(o => o.Name));
                    model.Sections.ForEach(
                        x => x.Contents = sectionContentService.GetPagesBySection(x.ConvertTo<Domain.Cms.Section>(), true, false, int.MaxValue, 0, out total));
                }
            }
            return model;
        }

        public object Get(GetNodes request)
        {
            var query = Db.From<Domain.Cms.SectionNode>()
                .Where(w => w.TenantId == Session.TenantId && w.ParentId == request.Id)
                .OrderBy(o => o.ListIndex)
                .ThenBy(o => o.Name); //TODO ver los distintos order by
            var model = Db.Select<SectionNodeResult>(query);
            return model;
        }

        public object Get(LookupSections request)
        {
            var query = Db.From<Domain.Cms.Section>();
            query.Where(w => w.TenantId == Session.TenantId);
            if (request.Id.HasValue)
            {
                query.Where(w => w.Id == request.Id.Value);
            }

            else
            {
                if (!string.IsNullOrEmpty(request.Q))
                {
                    var tokens = request.Q.Split(' ');
                    foreach (var token in tokens)
                    {
                        int intToken;
                        if (int.TryParse(token, out intToken))
                        {
                            query.Where(x => x.Name.Contains(token));
                        }
                        else
                        {
                            query.Where(x => x.Name.Contains(token));
                        }
                    }
                }
            }

            var count = Db.Count(query);

            query = query.OrderByDescending(q => q.Id)
                .Limit(request.Page - 1 ?? 0, request.PageSize.GetValueOrDefault(100) * request.Page.GetValueOrDefault(1));


            var result = new LookupResult
            {
                Data = Db.Select(query).Select(x => new LookupItem { Id = x.Id, Text = x.Name }),
                Total = (int)count
            };
            return result;
        }

        [Authenticate]
        public object Post(SectionRenameRequest request)
        {
            var current = Db.SingleById<Domain.Cms.Section>(request.Id);
            current.Name = request.Name;
            current.UpdatedOn = DateTime.UtcNow;
            Db.Update(current);
            return request;
        }

        [Authenticate]
        public object Post(SectionMoveRequest request)
        {
            Db.Execute(string.Format("EXEC MoveSection {0}, {1}, {2}", request.Id, request.TargetId, request.Position));
            return true;
        }


        [Authenticate]
        public object Post(PostSection request)
        {
            request.CreatedOn = DateTime.UtcNow;
            request.UpdatedOn = DateTime.UtcNow;
            request.TenantId = Session.TenantId;
            request.Id = (int)Db.Insert((Domain.Cms.Section)request, true);
            return request;
        }

        [Authenticate]
        public object Put(PostSection request)
        {
            request.UpdatedOn = DateTime.UtcNow;
            Db.Update(request);
            return request;
        }

        [Authenticate]
        public void Delete(DeleteSection request)
        {
            Db.DeleteById<Domain.Cms.Section>(request.Id);
        }
    }
}