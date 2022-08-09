using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CentralOperativa.Domain.Loans;
using ServiceStack;
using ServiceStack.Logging;
using ServiceStack.OrmLite;
using Api = CentralOperativa.ServiceModel.Loans;

namespace CentralOperativa.ServiceInterface.Loans
{
    [Authenticate]
    public class LoanScoreService : ApplicationService
    {
        public static ILog Log = LogManager.GetLogger(typeof(LoanScoreService));
        private readonly Infraestructure.AzureService _azureService;
        private readonly LoanRepository _loanRepository;

        public LoanScoreService(
            Infraestructure.AzureService azureService,
            LoanRepository loanRepository)
        {
            _azureService = azureService;
            _loanRepository = loanRepository;
        }

        public async Task<bool> Post(Api.PostCreditScoreRequest request)
        {
            try
            {
                var url = "http://arventubu1.centraloperativa.com:5200/api/evaluacioncrediticia/evaluarcredito";
                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.PostAsync(url, new StringContent(request.ToJson(), Encoding.UTF8, "application/json"));
                var responseBody = await response.Content.ReadAsStringAsync();

                var scoring = new LoanScore
                {
                    PersonId = request.PersonId,
                    LoanId = request.LoanId,
                    LoanLenderId = request.LoanLenderId,
                    CreateDate = DateTime.UtcNow,
                    Data = responseBody
                };

                var argentineCulture = new global::System.Globalization.CultureInfo("es-ar");
                if (response.IsSuccessStatusCode)
                {
                    var scoringResponse = responseBody.FromJson<Api.BancoDeComercioScoringReponse>();
                    if (scoringResponse != null)
                    {
                        scoring.Accepted = scoringResponse.Status == "ok";
                        scoring.Status = 2;
                        scoring.Score = decimal.Parse(scoringResponse.Score, argentineCulture);
                        scoring.RCI = decimal.Parse(scoringResponse.RCI, argentineCulture);
                        scoring.ApprovedAmount = decimal.Parse(request.ImporteCuota, argentineCulture) * request.Term; // Ver esto
                        scoring.Installment = decimal.Parse(request.ImporteCuota, argentineCulture);
                        scoring.DueDate = DateTime.UtcNow.AddMonths(request.Term);
                        scoring.Comments = scoringResponse.Descripcion;
                        scoring.Result = scoringResponse.Resultado;
                    }
                }
                else
                {
                    scoring.Accepted = false;
                    scoring.Status = 1;
                }
                scoring.Id = (int)Db.Insert(scoring, true);

                //Log request/response
                var cloudTable = await _azureService.FindOrCreateTable("bancodecomercioevaluarcredito");
                var insertEntity = new BancoDeComercioEvaluarCreditoEntity(scoring.Id, request.ToJson(), responseBody);
                var insertOperation = Microsoft.WindowsAzure.Storage.Table.TableOperation.Insert(insertEntity);
                await cloudTable.ExecuteAsync(insertOperation);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public class BancoDeComercioEvaluarCreditoEntity : Microsoft.WindowsAzure.Storage.Table.TableEntity
        {
            public BancoDeComercioEvaluarCreditoEntity(int id, string request, string response)
            {
                RowKey = id.ToString();
                PartitionKey = string.Empty;
                Request = request;
                Response = response;
            }

            public string Request { get; set; }

            public string Response { get; set; }
        }

        public QueryResponse<Api.QueryCreditScoresResult> Get(Api.QueryLoanScores request)
        {
            var loan = _loanRepository.GetLoan(Db, Session, request.LoanGuid, true);
            var q = Db.From<LoanLender>()
            .LeftJoin<LoanLender, LoanScore>((ll, ls) => ll.Id == ls.LoanLenderId && ls.LoanId == loan.Id)
            .Select<LoanLender, LoanScore>((ll, ls) => new {
                ls.Id,
                LoanLenderId = ll.Id,
                LoanLenderName = ll.Name,
                ls.Status,
                ls.Result,
                ls.Score,
                ls.RCI,
                ls.ApprovedAmount,
                ls.Installment,
                ls.DueDate,
                ls.CreateDate,
                ls.Comments,
                ls.Accepted
            });
            var data = Db.Select<Api.QueryCreditScoresResult>(q);
            var model = new QueryResponse<Api.QueryCreditScoresResult>()
            {
                Results = data,
                Total = data.Count
            };
            return model;
        }

        public QueryResponse<Api.QueryCreditScoresResult> Get(Api.QueryPersonScores request)
        {
            var q = Db.From<LoanLender>()
            .LeftJoin<LoanLender, LoanScore>((ll, ls) => ll.Id == ls.LoanLenderId && ls.PersonId == request.PersonId);
            var data = Db.Select<Api.QueryCreditScoresResult>(q);
            var model = new QueryResponse<Api.QueryCreditScoresResult>()
            {
                Results = data,
                Total = data.Count
            };
            return model;
        }
    }
}