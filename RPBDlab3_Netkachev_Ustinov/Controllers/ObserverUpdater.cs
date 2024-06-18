using Microsoft.AspNetCore.SignalR;

namespace RPBDlab3_Netkachev_Ustinov.Controllers
{
    public class ObserverUpdater : Hub
    {
        public async Task SendUpdateNotification()
        {
            await Clients.All.SendAsync("UpdateReceived");
        }
    }
}