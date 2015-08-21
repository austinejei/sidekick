using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace SmsShortcodeHandler
{
    public class SmsResult : IHttpActionResult
    {
        public HttpStatusCode HttpStatusCode { get; set; }
        public string Message { get; set; }

        public SmsResult(HttpStatusCode httpStatusCode, string message)
        {
            HttpStatusCode = httpStatusCode;
            Message = message;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {

            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode)
                                           {
                                               Content =
                                                   new StringContent(Message,
                                                   Encoding.Unicode, "text/plain"),
        
                                           };

            return Task.FromResult(response);
        }
    }
}