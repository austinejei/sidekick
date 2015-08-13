using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Api.Events
{
    [HubName("notificationPublisher")]
    public class NotificationPublisher : Hub
    {

    }
}