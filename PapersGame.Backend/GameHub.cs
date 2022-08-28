using Microsoft.AspNetCore.SignalR;
using PapersGame.Backend.Domain;
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

        public async Task UserRequestsPlayer()
        {
            await Clients.Caller.SendAsync("ReceivePlayer", _gameProvider.Game.GetPlayer(Context.ConnectionId));
        }
        #endregion

        public GameHub()
        {

        }
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception? exception)
        {
            return base.OnDisconnectedAsync(exception);
        }

        public async Task CreateGame(string gameName, int playerCount)
        {
            try
            {
                _gameProvider.CreateGame(gameName, playerCount, Context.ConnectionId);
                
                await Clients.Caller.SendAsync("IGameCreated", _gameProvider.Game);
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "CreateGame: " + ae.Message);
                throw;
            }
        }

        public async Task JoinToGame(string userName, string gameId)
        {
            try
            {
                _gameProvider.JoinToGame(userName, Context.ConnectionId);

                var game = GetGameByConnectionId(Context.ConnectionId);
                if (game is not null)
                {
                    var gameName = game.Name;
                    await Groups.AddToGroupAsync(Context.ConnectionId, gameName);

                    await Clients.Caller.SendAsync("IGameJoined", _gameProvider.Game);
                    await Clients.Group(gameName).SendAsync("RecivePlayersList", game.Players);
                }
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "JoinToGame: " + ae.Message);
                throw;
            }
        }

        public async Task SetPlayerReady(string characterName)
        {
            try
            {
                _gameProvider.SetPlayerReady(Context.ConnectionId, characterName);

                var game = GetGameByConnectionId(Context.ConnectionId);
                if(game is not null)
                {
                    await Clients.Group(game.Name).SendAsync("RecivePlayersList", game.Players);
                }
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "SetPlayerReady: " + ae.Message);
                throw;
            }
        }

        public async Task SetPlayerUnready()
        {
            try
            {
                _gameProvider.SetPlayerUnready(Context.ConnectionId);

                var game = GetGameByConnectionId(Context.ConnectionId);
                if (game is not null)
                {
                    await Clients.Group(game.Name).SendAsync("RecivePlayersList", game.Players);
                }
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

        #region Private methods
        private Game? GetGameByConnectionId(string connectionId)
        {
            //TODO: use connection id to find game
            return _gameProvider.Game;
        }
        #endregion
    }
}
