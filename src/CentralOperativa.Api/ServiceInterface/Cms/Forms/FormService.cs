using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CentralOperativa.Infraestructure;
using CentralOperativa.ServiceInterface.System.Persons;
using ServiceStack;
using ServiceStack.OrmLite;
using ServiceStack.OrmLite.Dapper;
using ServiceStack.Text;
using Api = CentralOperativa.ServiceModel.Cms.Forms;

namespace CentralOperativa.ServiceInterface.Cms.Forms
{
    
    public class FormService : ApplicationService
    {
        private IAutoQueryDb _autoQuery;
        private PersonRepository _personRepository;
        private FormRepository _formRepository;

        public FormService(
            IAutoQueryDb autoQuery,
            PersonRepository personRepository,
            FormRepository formRepository)
        {
            _autoQuery = autoQuery;
            _personRepository = personRepository;
            _formRepository = formRepository;
        }

        private class FormFieldMap
        {
            public FormFieldMap()
            {
                this.Options = new List<OptionMap>();
            }

            public int FormId { get; set; }

            public int FieldIndex { get; set; }
            public string OldFieldId { get; set; }

            public string NewFieldId { get; set; }

            public List<OptionMap> Options { get; private set; }
        }

        private class OptionMap
        {
            public int Index { get; set; }

            public string Text { get; set; }
            public string OldId { get; set; }

            public string NewId { get; set; }
        }

        public class FormField
        {
            public string Id { get; set; }

            public string Name { get; set; }

            public string Type { get; set; }

            public bool Required { get; set; }

            public string Value { get; set; }

            public List<Option> Options { get; set; }
        }

        public class Option
        {
            public string Text { get; set; }

            public string Value { get; set; }
        }

        [Authenticate]
        public async Task<object> Get(Api.GetFormResults request)
        {
            return await ExportForm(request.Id);
        }

        [Authenticate]
        private async Task<ExcelFileResult> ExportForm(int id)
        {
            var result = new DataTable();
            var form = Db.Query<Domain.Cms.Forms.Form>("SELECT * FROM Forms WHERE Id = " + id).SingleOrDefault();
            if (form == null)
            {
                throw new ApplicationException(string.Format("The form with id {0} does not exist", id));
            }

            var query = "SELECT fr.* FROM FormResponses fr LEFT OUTER JOIN Persons p ON p.Id = fr.PersonId WHERE FormId = " + id + " ORDER BY p.Name";
            var formResposes = Db.Query<Domain.Cms.Forms.FormResponse>(query).ToList();

            result.Columns.Add(new DataColumn { ColumnName = "Id" });
            result.Columns.Add(new DataColumn { ColumnName = "Estado" });
            result.Columns.Add(new DataColumn { ColumnName = "Fecha" });
            result.Columns.Add(new DataColumn { ColumnName = "Mes" });
            result.Columns.Add(new DataColumn { ColumnName = "Año" });
            if (form.ValidateUser)
            {
                result.Columns.Add(new DataColumn { ColumnName = "Apellido" });
                result.Columns.Add(new DataColumn { ColumnName = "Nombre" });
                result.Columns.Add(new DataColumn { ColumnName = "Email" });
                result.Columns.Add(new DataColumn { ColumnName = "Telefono" });
                result.Columns.Add(new DataColumn { ColumnName = "Celular" });
                result.Columns.Add(new DataColumn { ColumnName = "Direccion" });
                //result.Columns.Add(new DataColumn { ColumnName = "TipoDocumento" });
                result.Columns.Add(new DataColumn { ColumnName = "Documento" });
            }


            List<FormField> fields = JsonSerializer.DeserializeFromString<List<FormField>>(form.Fields);
            for (var fieldIndex = 0; fieldIndex < fields.Count; fieldIndex++)
            {
                var field = fields[fieldIndex];
                var columnName = string.Format("{0}-{1}", fieldIndex, field.Name);
                result.Columns.Add(new DataColumn { ColumnName = columnName.Replace(":", string.Empty) });
            }

            foreach (var formResponse in formResposes)
            {
                var colIndex = 0;
                var row = result.NewRow();
                row[colIndex] = formResponse.Id;
                row[++colIndex] = Enum.GetName(typeof(Domain.Cms.Forms.FormResponseStatus), formResponse.StatusId);
                row[++colIndex] = formResponse.StartDate;
                row[++colIndex] = formResponse.StartDate.Value.Month;
                row[++colIndex] = formResponse.StartDate.Value.Year;

                if (form.ValidateUser)
                {
                    if (formResponse.PersonId.HasValue)
                    {
                        var person = await _personRepository.GetPerson(Db, formResponse.PersonId.Value);
                        row[++colIndex] = person.LastName;
                        row[++colIndex] = person.FirstName;
                        row[++colIndex] = person.Emails.FirstOrDefault()?.Address;
                        row[++colIndex] = person.Phones.Where(x => x.TypeId == 1).FirstOrDefault();
                        row[++colIndex] = person.Phones.Where(x => x.TypeId == 4).FirstOrDefault();
                        row[++colIndex] = person.Addresses.FirstOrDefault()?.Address.Name;
                        row[++colIndex] = person.Code;
                    }
                    else
                    {
                        colIndex = colIndex + 7;
                    }
                }

                var fieldValues = JsonObject.ParseArray(formResponse.Fields);
                for (var fieldIndex = 0; fieldIndex < fields.Count; fieldIndex++)
                {
                    var field = fields[fieldIndex];
                    JsonObject fieldValue = null;
                    foreach (var fieldValueItem in fieldValues)
                    {
                        if (fieldValueItem["id"] == field.Id)
                        {
                            fieldValue = fieldValueItem;
                            break;
                        }
                    }

                    if (fieldValue == null)
                    {
                        row[++colIndex] = null;
                    }
                    else
                    {
                        if (field.Type == "radio" || field.Type == "checkbox" || field.Type == "dropdown")
                        {
                            if (fieldValue.ContainsKey("value"))
                            {
                                var values = fieldValue["value"].FromJson<string[]>();
                                if (values != null && values.Length > 0)
                                {
                                    if (field.Options != null)
                                    {
                                        var value = values[0].ToString(CultureInfo.InvariantCulture);
                                        var option = field.Options.SingleOrDefault(x => x.Value == value);
                                        if (option != null)
                                        {
                                            row[++colIndex] = option.Text;
                                        }
                                        else
                                        {
                                            row[++colIndex] = value;
                                        }
                                    }
                                    else
                                    {
                                        row[++colIndex] = fieldValue["value"];
                                    }
                                }
                                else
                                {
                                    row[++colIndex] = null;
                                }
                            }
                            else
                            {
                                row[++colIndex] = fieldValue["value"];
                            }
                        }
                        else
                        {
                            row[++colIndex] = fieldValue["value"];
                        }
                    }
                }

                result.Rows.Add(row);
            }

            throw new NotImplementedException("Port this to report service");
            /*
            var license = new Aspose.Cells.License();
            license.SetLicense(Infraestructure.License.LStream);

            var wb = new Workbook();
            wb.Worksheets.Clear();
            var ws = wb.Worksheets.Add("Formulario " + id);
            ws.Cells.ImportDataTable(result, true, 0, 0);
            var ms = wb.SaveToStream();
            var fileContentsResult = new ExcelFileResult(ms, string.Format("Resultados encuesta {0}", id));
            return fileContentsResult;
            */
        }

        [Authenticate]
        public async Task<Api.GetFormResult> Get(Api.GetForm request)
        {
            return await _formRepository.GetForm(Db, request.Id);
        }

        public async Task<Api.GetFormResult> Get(Api.GetFormByGuid request)
        {
            return await _formRepository.GetForm(Db, request.Guid);
        }

        [Authenticate]
        public QueryResponse<Api.QueryFormsResult> Get(Api.QueryForms request)
        {
            if (request.OrderBy == null)
            {
                request.OrderBy = "Id";
            }

            var q = _autoQuery.CreateQuery(request, Request.GetRequestParams());
            q.Where(x => x.TenantId == Session.TenantId); // && x.TypeId == 1

            return _autoQuery.Execute(request, q);

            /*
            var select = new Select();
            select.Fields.Add(new Field("f", "Id", "id"));
            select.Fields.Add(new Field("f", "Name", "name"));
            select.Fields.Add(new Field("f", "FromDate", "fromDate"));
            select.Fields.Add(new Field("f", "ToDate", "toDate"));
            select.Fields.Add(new Field("f", "Responses", "responses"));
            select.Fields.Add(new Field("f", "MinQuota", "minQuota"));
            select.Fields.Add(new Field("f", "Quota", "quota"));
            select.From.Add("vForm f");

            if (!string.IsNullOrEmpty(request.Q))
            {
                select.Where.Add(string.Format("f.Name LIKE '{0}'", Utils.ToSqlLike(request.Q)));
            }

            if (request.ShowOnlyActive)
            {
                select.Where.Add("f.FromDate <= " + Utils.DatetoSql(DateTime.UtcNow));
                select.Where.Add("f.ToDate >= " + Utils.DatetoSql(DateTime.UtcNow));
            }

            select.OrderBy.Add("f.Id DESC");
            var query = new JqGridSqlQuery<dynamic>(select, Db);
            var model = query.Execute();
            return Request.ToOptimizedResult(model);
            */
        }

        [Authenticate]
        public LookupResult Get(Api.LookupFormsRequest request)
        {
            var result = GetForms(request.ShowOnlyActive, request.Page - 1, request.PageSize, request);
            return new LookupResult
            {
                Data = result.Select(x => new LookupItem
                {
                    Id = x.Id,
                    Text = x.Name
                }),
                Total = result.Count
            };
        }

        [Authenticate]
        public async Task<Api.GetFormResult> Post(Api.CopyForm request)
        {
            var form = (await _formRepository.GetForm(Db, request.Id)).ConvertTo<Api.PostForm>();
            form.Id = 0;
            form.TenantId = Session.TenantId;
            form.IsPublished = false;
            form.Name = request.Name;
            form.Guid = Guid.NewGuid();
            return await Post(form);
        }

        [Authenticate]
        public async Task<Api.GetFormResult> Post(Api.PostForm request)
        {
            Api.GetFormResult model;
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    model = await _formRepository.CreateForm(Db, request, Session.TenantId);
                    trx.Commit();
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
            return model;
        }

        [Authenticate]
        public async Task<Api.GetFormResult> Put(Api.PostForm request)
        {
            Api.GetFormResult model;
            using (var trx = Db.OpenTransaction())
            {
                try
                {
                    model = await _formRepository.UpdateForm(Db, request);
                    trx.Commit();
                }
                catch (Exception)
                {
                    trx.Rollback();
                    throw;
                }
            }
            return model;
        }

        [Authenticate]
        public void Delete(Api.DeleteFrom request)
        {
            Db.DeleteById<Domain.Cms.Forms.Form>(request.Id);
        }

        internal class FormData
        {
            public int Id { get; set; }

            public int Quota { get; set; }

            public int Responses { get; set; }

            public bool ValidateUser { get; set; }

            public DateTime StartDate { get; set; }

            public dynamic Fields { get; set; }
        }

        internal class FormResponseData
        {
            public int Id { get; set; }

            public DateTime StartDate { get; set; }

            public string Code { get; set; }

            public Guid Guid { get; set; }

            public byte StatusId { get; set; }

            public dynamic Fields { get; set; }

            public int? ContactId { get; set; }

            public string LastName { get; set; }

            public string FirstName { get; set; }

            public string Email { get; set; }

            public string Phone { get; set; }

            public string Mobile { get; set; }

            public string Address { get; set; }

            public string StreetName { get; set; }

            public string BuildingNumber { get; set; }

            public string Floor { get; set; }

            public string Department { get; set; }

            public string City { get; set; }

            public string State { get; set; }

            public string TipoMatricula { get; set; }

            public int? Matricula { get; set; }

            public string TipoDocumento { get; set; }

            public string Documento { get; set; }
        }

        private List<Domain.Cms.Forms.Form> GetForms(bool showOnlyActive, int? pageIndex = null, int? pageSize = null, Api.LookupFormsRequest request = null)
        {
            var visitor = Db.From<Domain.Cms.Forms.Form>();

            if (request.Id.HasValue)
            {
                visitor.Where(w => w.Id == request.Id.Value);
            }
            else
            {
                visitor.Where(x => x.Name.Contains(request.Q));
            }
            
            if (showOnlyActive)
            {
                visitor.And(x => DateTime.UtcNow >= x.FromDate);
                visitor.And(x => DateTime.UtcNow <= x.ToDate);
            }

            if(!string.IsNullOrEmpty(request.Filter))
            {
                if (request.Filter == "only_landings")
                {
                    visitor.And(x => x.TypeId == 2);
                }

                if (request.Filter == "only_forms")
                {
                    visitor.And(x => x.TypeId == 0);
                }
            }
            
            visitor.And(x => x.TenantId == Session.TenantId);

            var countStatement = visitor.ToCountStatement();

            if (pageIndex.HasValue && pageSize.HasValue)
            {
                visitor.Limit(pageIndex.Value * pageSize.Value, pageSize.Value);
            }

            visitor.OrderByDescending(x => x.Id);
            var result = Db.Select(visitor);
            return result;
        }
    }
}