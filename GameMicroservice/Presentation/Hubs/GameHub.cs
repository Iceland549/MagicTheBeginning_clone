using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace GameMicroservice.Presentation.Hubs
{
    public class GameHub : Hub
    {   
        public async Task JoinGame(string gameId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, gameId);
            await Clients.Group(gameId).SendAsync("PlayerJoined", gameId);
        }

        public async Task LeaveGame(string gameId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, gameId);
            await Clients.Group(gameId).SendAsync("PlayerLeft", gameId);
        }
    }
}
