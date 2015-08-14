using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace Api.Events
{
    public class SidekickActivitySignaler
    {
        private static IHubContext _hubContext;
        public SidekickActivitySignaler()
        {
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationPublisher>();
        }

 

        public async Task ReportAsLiveFeed(string timeStamp,string message)
        {
            _hubContext.Clients.All.liveFeed(timeStamp, message);

            await Task.Delay(0);
        }
    }
}