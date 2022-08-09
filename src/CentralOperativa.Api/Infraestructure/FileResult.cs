using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ServiceStack;
using ServiceStack.Web;

namespace CentralOperativa.Infraestructure
{
    public class FileResult : IDisposable, IStreamWriterAsync, IHasOptions
    {
        private readonly MemoryStream _responsestream ;
        public IDictionary<string, string> Options { get; set; }

        public FileResult(MemoryStream responseStream, string contentType, string fileName)
        {
            _responsestream = responseStream;
            Options = new Dictionary<string, string>
        {
            { HttpHeaders.ContentLength, _responsestream.Length.ToString() },
            { HttpHeaders.ContentType, contentType},
            { HttpHeaders.ContentDisposition, $"inline; filename=\"{fileName}\";"}
        };
        }

        public async Task WriteToAsync(Stream responseStream, CancellationToken token = new CancellationToken())
        {
            if (_responsestream == null)
                return;

            await _responsestream.CopyToAsync(responseStream, token);
            await _responsestream.FlushAsync(token);
        }

        public void Dispose()
        {
            _responsestream.Dispose();
        }
    }
}
