using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace ApiThrottling
{
    public class VerboseRequestLogger:DelegatingHandler
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            
           
                Logger.Debug("Url: {0} \r\n Method: {1} \r\n Headers: {2} \r\n Body: {3} \r\n",request.RequestUri,request.Method,request.Headers,request.Content.ReadAsStringAsync().Result);



            return base.SendAsync(request, cancellationToken).ContinueWith(c =>
                                                                           {
                                                                               
                                                                               var response = c.Result;

                                                                               var rawResponse = string.Empty;
                                                                               try
                                                                               {
                                                                                   rawResponse =
                                                                                       response.Content
                                                                                           .ReadAsStringAsync().Result;
                                                                               }
                                                                               catch (Exception exception)
                                                                               {
                                                                                 //  Logger.Warn(exception.Message);
                                                                               }
                                                                               Logger.Debug("Response => Headers: {0} \r\n Body: {1}",response.Headers,rawResponse);
                                                                               return response;
                                                                           }, cancellationToken);
        }

        
       
    }
}
