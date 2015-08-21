using System.Threading.Tasks;
using Api.Events;
using NLog;

namespace ApiEventHandler
{
    public class AfterEmailSentEventhandler : IEventhandlerModule
    {

        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public void Initialize(SidekickEvents events)
        {
            events.EmailSent += AfterEmailSent;
        }

        private async Task AfterEmailSent(EmailSentEventArgs e)
        {
            Log.Info("event handler worked!!");
            await Task.Delay(0);
        }


      
    }
}