using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace Api.Events
{
    public class CashBaggActivitySignaler
    {
        private static IHubContext _hubContext;
        public CashBaggActivitySignaler()
        {
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<NotificationPublisher>();
        }

        public async Task OnNewCustomerCreated(string timeStamp)
        {
            _hubContext.Clients.All.newCustomerCreated(timeStamp);

            await Task.Delay(0);
        }

        public async Task UpdateChart(string redeemedDay)
        {
            _hubContext.Clients.All.updateChartLive(redeemedDay);

            await Task.Delay(0);
        }

        public async Task ReportAsLiveFeed(string timeStamp,string message, string customerName,string customerPhone)
        {
            _hubContext.Clients.All.liveFeed(timeStamp, message, customerName, customerPhone);

            await Task.Delay(0);
        }
    }
}