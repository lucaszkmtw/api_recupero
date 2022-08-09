using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using Api = CentralOperativa.ServiceModel.Cms;

namespace CentralOperativa.ServiceInterface.Cms
{
    [Authenticate]
    public class SectionContentService : Service
    {
        public object Get(Api.GetSectionContent request)
        {
            /*
            SectionContent result = null;
            if (request.Id != default(int))
            {
                result = this.cmsService.GetSectionContentById(request.Id).ConvertTo<SectionContent>();
            }

            return result;
            */

            throw new NotImplementedException();
        }

        public object Get(Api.SearchSectionContents request)
        {
            var limit = request.PageSize ?? 100;
            var page = request.Page ?? 1;
            var offset = (page - 1) * limit;

            if (request.SectionId.HasValue)
            {
                var section = Db.SingleById<Domain.Cms.Section>(request.SectionId.Value);
                int total;
                var result = this.GetPagesBySection(section, request.OnlyPublished, request.ChildSections, limit, offset, out total);
                if (!request.OnlyTitle)
                {
                    return new { Data = result, Total = total };
                }
                else
                {
                    var items = result.Select(
                        x =>
                            new Api.GetSectionContentTitleResponse
                            {
                                ContentId = x.Content.Id,
                                DisplayOrder = x.DisplayOrder,
                                Highlighted = x.Highlighted,
                                SectionId = x.Section.Id,
                                Title = x.Content.Title,
                                Id = x.Id
                            }).ToList();
                    return new { Data = items, Total = total };
                }
            }

            return this.Search(request.SectionId, request.Page - 1, request.PageSize);
        }

        public List<Api.GetSectionContentResponse> GetPagesBySection(Domain.Cms.Section section, bool onlyPublished, bool childSections, int limit, int offset, out int total)
        {
            var from = "SectionContents sc INNER JOIN Contents c ON c.Id = sc.ContentId WHERE sc.SectionId = @sectionId";
            if (onlyPublished)
            {
                from += " AND c.PublishDate <= GETUTCDATE() AND (c.ExpirationDate IS NULL OR c.ExpirationDate >= GETUTCDATE())";
            }

            var countSql = string.Format("SELECT COUNT(*) FROM {0}", from);
            total = Db.Scalar<int>(countSql, new { sectionId = section.Id });

            string orderBy = string.Empty;
            switch (section.OrderItemsBy)
            {
                case 1:
                    orderBy += " ORDER BY c.PublishDate DESC, sc.DisplayOrder";
                    break;
                case 2:
                    orderBy += " ORDER BY c.PublishDate ASC, sc.DisplayOrder";
                    break;
                case 3:
                    orderBy += " ORDER BY c.Title ASC, sc.DisplayOrder";
                    break;
                case 4:
                    orderBy += " ORDER BY c.Title DESC, sc.DisplayOrder";
                    break;
                default:
                    orderBy += " ORDER BY sc.DisplayOrder";
                    break;
            }

            var fields = @"sc.Id, sc.DisplayOrder, sc.Highlighted, c.*";
            var sql = string.Format("SELECT {0} FROM {1} {2} OFFSET {3} ROWS FETCH NEXT {4} ROWS ONLY", fields, from, orderBy, offset, limit);

            var result = Db.Query<Api.GetSectionContentResponse, Domain.Cms.Content, Api.GetSectionContentResponse>(sql, (page, content) =>
            {
                page.Content = content;
                page.Section = section;
                return page;
            }, new { sectionId = section.Id }, null, false, "Id", null, null).ToList();

            /*
            if (childSections)
            {
                var children = this.GetSectionsByParent(section.Id, null);
                foreach (var childSection in children)
                {
                    result.AddRange(this.GetPagesBySection(childSection, onlyPublished, childSections));
                }
            }
            */

            return result;
        }

        public object Get(Api.LookupSectionContents request)
        {
            /*
            var items = cmsService.LookupPage(request.Page - 1, request.PageSize, request.Q, request.Id);
            return new LookupResult
            {
                Data = items.Select(x => new LookupItem
                {
                    Id = x.Id,
                    Text = x.Name
                }),
                Total = items.TotalCount
            };
            */
            throw new NotImplementedException();
        }

        private object Search(int? sectionId, int? pageIndex, int? pageSize)
        {
            /*
            PagedList<SectionContent> result;
            if (sectionId.HasValue)
            {
                var sectionIdValue = sectionId.Value;
                result = new PagedList<SectionContent>(
                    Db.Select<SectionContent>(x => x.SectionId == sectionIdValue),
                    pageIndex,
                    pageSize);
            }
            else
            {
                result = new PagedList<SectionContent>(
                    Db.Select<SectionContent>(),
                    pageIndex,
                    pageSize);
            }

            return result;
            */
            throw new NotImplementedException();
        }

        [Authenticate]
        public object Post(Api.PostSectionContent request)
        {
            return this.SaveSectionContent(request);
        }

        [Authenticate]
        public object Put(Api.PostSectionContent request)
        {
            return this.SaveSectionContent(request);
        }

        [Authenticate]
        public void Post(Api.MoveSectionContent request)
        {
            this.MoveSectionContent(request.SourceId, request.TargetId, request.Position);
        }

        [Authenticate]
        public object Post(Api.HighlightSectionContent request)
        {
            var sectionContent = Db.SingleById<Domain.Cms.SectionContent>(request.Id);
            sectionContent.Highlighted = request.Value;
            this.SaveSectionContent(sectionContent);
            return Request;
        }

        [Authenticate]
        public void Delete(Api.DeleteSectionContent request)
        {
            Db.DeleteById<Domain.Cms.SectionContent>(request.Id);
        }

        public Domain.Cms.SectionContent SaveSectionContent(Domain.Cms.SectionContent item)
        {
            var isNew = item.Id == 0;
            var section = Db.SingleById<Domain.Cms.Section>(item.SectionId);
            int total;
            var pages = this.GetPagesBySection(section, false, false, int.MaxValue, 0, out total).OrderBy(x => x.DisplayOrder).ToList();

            if (isNew)
            {
                item.DisplayOrder = (short)(pages.Count == 0 ? 0 : (pages.Max(x => x.DisplayOrder) + 1));
            }

            Db.Save(item);
            if (isNew)
            {
                if (pages.Count > 0)
                {
                    this.MoveSectionContent(item.Id, pages[0].Id, 1);
                }
            }

            return item;
        }

        public int MoveSectionContent(int sourceId, int targetId, int position)
        {
            var result = Db.Execute(string.Format("EXEC MoveSectionContent {0}, {1}, {2}", sourceId, targetId, position));
            return result;
        }
    }
}