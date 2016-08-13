using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using Api.Events;
using ApiHandlers.CustomAttributes;
using ApiHandlers.Models;
using DataLayer;
using NLog;

namespace ApiHandlers
{
    /// <summary>
    /// Responsible for all email requests
    /// </summary>
    [Authorize, RoutePrefix("v1/emails")]
    public class EmailsController : ApiController
    {
        protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

      /// <summary>
        /// Lists profile about a user
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost, Route, OAuthScope("emails.send")]
        public async Task<IHttpActionResult> ListUserProfile(EmailModel model)
        {
            if (model==null)
            {
                return BadRequest("invalid request body");
            }

            Logger.Debug("received request to send email");

            var userIdentity = User.Identity as ClaimsIdentity;

            var appName = userIdentity.Claims.FirstOrDefault(c => c.Type == "sidekick.client.appName").Value;

            await Task.Delay(4000); //intentional delay
            Logger.Debug("email sent");
            Logger.Debug("raising events after sending email");
            await SidekickEventsManager.Instance.Events.OnEmailSent(new EmailSentEventArgs
            {
                Body = model.Body,
                Subject = model.Title,
                Process = appName,
                Receipient = model.To,
                Sender = userIdentity.Name
            });

            Logger.Debug("sending signalr event to connected clients");
            await
                SidekickEventsManager.Instance.ActivitySignaler.ReportAsLiveFeed(DateTime.Now.ToString("U"),
                    string.Format("{0} has just sent an email to {1}",appName,model.To));


            Logger.Debug("done!");
            return Ok(new
                      {
                          Id = Guid.NewGuid().ToString()
                      });
        }
    }
}