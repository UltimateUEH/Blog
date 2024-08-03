using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace AppMVCWeb.Hubs
{
    public class ChatHub : Hub
    {
        private static Random random = new Random();

        public async Task SendMessage(string message)
        {
            string user;
            string role;

            if (Context.User.Identity.IsAuthenticated)
            {
                user = Context.User.Identity.Name;
                role = "User";
            }
            else
            {
                user = "Anonymous#" + random.Next(0, 9999999);
                role = "Anonymous";
            }

            var timestamp = DateTime.Now.ToString("hh:mm tt");
            var formattedMessage = $"{user} - {timestamp}: {message}";

            await Clients.All.SendAsync("ReceiveMessage", role, formattedMessage);
        }
    }
}
