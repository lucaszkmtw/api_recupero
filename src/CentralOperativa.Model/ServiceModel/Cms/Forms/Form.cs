using System;
using System.Collections.Generic;
using CentralOperativa.Infraestructure;
using ServiceStack;

namespace CentralOperativa.ServiceModel.Cms.Forms
{
    [Route("/cms/forms/lookup", "GET")]
    public class LookupFormsRequest : LookupRequest, IReturn<List<LookupItem>>
    {
        public bool ShowOnlyActive { get; set; }
    }

    [Route("/cms/forms", "GET")]
    public class QueryForms : QueryDb<Domain.Cms.Forms.FormView, QueryFormsResult>
    {
        public bool ShowOnlyActive { get; set; }
    }

    public class QueryFormsResult : Domain.Cms.Forms.FormView
    {
    }

    [Route("/cms/forms/guid/{Guid}", "GET")]
    public class GetFormByGuid : IReturn<GetFormResult>
    {
        public Guid Guid { get; set; }
    }

    [Route("/cms/forms/{Id}", "GET")]
    public class GetForm : IReturn<GetFormResult>
    {
        public int Id { get; set; }
    }

    public class GetFormResult : Domain.Cms.Forms.FormView
    {
        public List<Domain.Cms.Forms.FormRole> Roles { get; set; }
        public GetFormResult()
        {
            this.Roles = new List<Domain.Cms.Forms.FormRole>();
        }
    }

    [Route("/cms/forms/{Id}/copy", "POST")]
    public class CopyForm : IReturn<GetFormResult>
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    [Route("/cms/forms", "POST")]
    [Route("/cms/forms/{Id}", "POST, PUT")]
    public class PostForm : Domain.Cms.Forms.Form, IReturn<GetFormResult>
    {
    }

    [Route("/cms/forms/{Id}", "DELETE")]
    public class DeleteFrom : IReturnVoid
    {
        public int Id { get; set; }
    }

    [Route("/cms/forms/{Id}/results", "GET")]
    public class GetFormResults : IReturn<ExcelFileResult>
    {
        public int Id { get; set; }
        public string Exporter { get; set; }
    }

    #region FormRoles
    [Route("/cms/forms/{FormId}/roles", "GET")]
    public class GetFormRoles : IReturn<GetFormRolesResult>
    {
        public int FormId { get; set; }
    }

    public class GetFormRolesResult : Domain.Cms.Forms.FormRole, IHasResponseStatus
    {
        public ResponseStatus ResponseStatus { get; set; }
    }

    [Route("/cms/forms/{FormId}/roles", "POST")]
    [Route("/cms/forms/{FormId}/roles/{Id}", "POST, PUT")]
    public class PostFormRole : Domain.Cms.Forms.FormRole, IReturn<GetFormRoleResult>
    {
    }

    [Route("/cms/forms/{FormId}/roles/{Id}", "DELETE")]
    public class DeleteFormRole : IReturnVoid
    {
        public int FormId { get; set; }
        public int Id { get; set; }
    }

    public class GetFormRoleResult : Domain.Cms.Forms.FormRole
    {
    }
    #endregion
}