using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using ServiceStack.Web;

namespace CentralOperativa.Infraestructure
{
    public class ExcelFileResult : IHasOptions, IStreamWriterAsync
    {
        private readonly Stream _responseStream;
        public IDictionary<string, string> Options { get; private set; }

        public ExcelFileResult(Stream responseStream, string fileName)
        {
            _responseStream = responseStream;

            Options = new Dictionary<string, string> {
             {"Content-Type", "application/octet-stream"},
             {"Content-Disposition", string.Format("attachment; filename=\"{0}.xls\";", fileName)}
         };
        }

        public async Task WriteToAsync(Stream responseStream, CancellationToken token = new CancellationToken())
        {
            if (_responseStream == null)
                return;

            await _responseStream.CopyToAsync(responseStream);
            //responseStream.Flush();
        }
    }
}
