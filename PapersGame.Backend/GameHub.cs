using Microsoft.AspNetCore.SignalR;
using PapersGame.Backend.Providers;

namespace PapersGame.Backend
{
    public class GameHub : Hub
    {
        private static readonly GameProvider _gameProvider = new GameProvider();

        #region Messages
        public async Task SendError(IClientProxy client, string errMessage)
        {
            await client.SendAsync("ReceiveError", errMessage);
        }
        #endregion

        public GameHub()
        {

        }
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public async Task CreateGame(string gameName, int playerCount)
        {
            try
            {
                _gameProvider.CreateGame(gameName, playerCount, Context.ConnectionId);
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "CreateGame: " + ae.Message);
            }
        }

        public async Task JoinToGame(string userName)
        {
            try
            {
                _gameProvider.JoinToGame(userName, Context.ConnectionId);
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "JoinToGame: " + ae.Message);
            }
        }

        public async Task SetPlayerReady(string characterName)
        {
            try
            {
                _gameProvider.SetPlayerReady(Context.ConnectionId, characterName);
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "SetPlayerReady: " + ae.Message);
            }
        }

        public async Task SetPlayerUnready()
        {
            try
            {
                _gameProvider.SetPlayerUnready(Context.ConnectionId);
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "SetPlayerUnready: " + ae.Message);
            }
        }

        public async Task StartGame()
        {
            try
            {
                _gameProvider.StartGame(Context.ConnectionId);
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "SetPlayerUnready: " + ae.Message);
            }
        }
    }
}
