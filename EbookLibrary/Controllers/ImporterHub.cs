using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace EbookLibrary.Controllers
{
    public class ImporterHub: Hub
    {

        public Task Send(string message)
        {
            return Clients.All.InvokeAsync("Send", message);
        }

    }
}
