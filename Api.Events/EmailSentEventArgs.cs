using System;

namespace Api.Events
{
    public class EmailSentEventArgs:EventArgs
    {
        public string Sender { get; set; }
        public string Receipient { get; set; }
        public string Body { get; set; }
        public string Process { get; set; } //or the app that sent it
    }
}