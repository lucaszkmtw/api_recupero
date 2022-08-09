using System;
using System.Data;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.Cms.Forms;

namespace CentralOperativa.ServiceInterface.Cms.Forms
{
    [Authenticate]
    public class FormRepository
    {
        public async Task<Api.GetFormResult> GetForm(IDbConnection db, Guid guid)
        {
            var item = await db.SingleAsync<Domain.Cms.Forms.FormView>(w => w.Guid == guid);
            if (item == null)
            {
                return null;
            }

            var response = item.ConvertTo<Api.GetFormResult>();
            response.Roles = await db.SelectAsync<Domain.Cms.Forms.FormRole>(x => x.FormId == response.Id);
            return response;
        }

        public async Task<Api.GetFormResult> GetForm(IDbConnection db, int id)
        {
            var item = db.SingleByIdAsync<Domain.Cms.Forms.FormView>(id);
            if (item == null)
            {
                return null;
            }

            var response = item.ConvertTo<Api.GetFormResult>();
            response.Roles = await db.SelectAsync<Domain.Cms.Forms.FormRole>(x => x.FormId == id);
            return response;
        }

        public async Task<Api.GetFormResult> CreateForm(IDbConnection db, Api.PostForm form, int tenantId)
        {
            if (form.Id == 0)
            {
                form.TenantId = tenantId;
                form.CreateDate = DateTime.UtcNow;
                form.Guid = Guid.NewGuid();
                form.Id = (int)db.Insert((Domain.Cms.Forms.Form)form, true);
            }
            else
            {
                await db.UpdateAsync((Domain.Cms.Forms.Form)form);
            }

            return form.ConvertTo<Api.GetFormResult>();
        }

        public async Task<Api.GetFormResult> UpdateForm(IDbConnection db, Api.PostForm form)
        {
            await db.SaveAsync((Domain.Cms.Forms.Form)form);
            return form.ConvertTo<Api.GetFormResult>();
        }

        public async Task DeleteForm(IDbConnection db, int id)
        {
            await db.DeleteByIdAsync<Domain.Cms.Forms.Form>(id);
        }
    }
}