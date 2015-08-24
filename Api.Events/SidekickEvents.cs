using System;
using System.Threading.Tasks;

namespace Api.Events
{
    public delegate Task SidekickEventsDelegate<T>(T e) where T:EventArgs;




    public class SidekickEvents
    {
        public SidekickEventsDelegate<EmailSentEventArgs> EmailSent { get; set; }



        public async Task OnEmailSent(EmailSentEventArgs eventArgs)
        {
            if (EmailSent!=null)
            {

                await EmailSent(eventArgs);
            }
        }

        
    }
}
