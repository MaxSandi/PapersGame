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
                    await Clients.Group(gameName).SendAsync("ReceivePlayersList", game.Players);
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
                    await Clients.Group(game.Name).SendAsync("ReceivePlayersList", game.Players);
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
                    await Clients.Group(game.Name).SendAsync("ReceivePlayersList", game.Players);
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

                var game = GetGameByConnectionId(Context.ConnectionId);
                if (game is not null)
                {
                    await Clients.Group(game.Name).SendAsync("IGameStarted", _gameProvider.Game);
                }

            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "StartGame: " + ae.Message);
            }
        }

        public async Task StopGame()
        {
            try
            {
                var game = GetGameByConnectionId(Context.ConnectionId);
                if (game is not null)
                {
                    var group = Clients.Group(game.Name);
                    _gameProvider.StopGame(Context.ConnectionId);

                    await group.SendAsync("IGameStopped");
                }
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "StartGame: " + ae.Message);
            }
        }

        public async Task<bool> CheckPlayerIsAdmin()
        {
            try
            {
                var game = GetGameByConnectionId(Context.ConnectionId);
                if (game is null)
                    throw new Exception();

                return Context.ConnectionId == game.AdminConnectionId;
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "CheckPlayerIsAdmin: " + ae.Message);
            }

            return false;
        }

        public async Task SetTurnNext()
        {
            try
            {
                var game = GetGameByConnectionId(Context.ConnectionId);
                if (game is null)
                    throw new Exception();

                var currentPlayerIndex = game.Players.FindIndex(x => x.Equals(game.CurrentPlayer));
                if (currentPlayerIndex == -1)
                    throw new Exception("Player not exist!");

                currentPlayerIndex++;
                if (currentPlayerIndex >= game.Players.Count)
                    currentPlayerIndex = 0;

                game.CurrentPlayer = game.Players[currentPlayerIndex];
                await Clients.Group(game.Name).SendAsync("ReceiveCurrentPlayer", currentPlayerIndex);
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "SetTurnNext: " + ae.Message);
            }
        }

        public async Task<int> SetTurnPrev()
        {
            try
            {
                var game = GetGameByConnectionId(Context.ConnectionId);
                if (game is null)
                    throw new Exception();

                var currentPlayerIndex = game.Players.FindIndex(x => x.Equals(game.CurrentPlayer));
                if(currentPlayerIndex == -1)
                    throw new Exception("Player not exist!");

                currentPlayerIndex--;
                if(currentPlayerIndex < 0)
                    currentPlayerIndex = game.Players.Count - 1;

                game.CurrentPlayer = game.Players[currentPlayerIndex];
                await Clients.Group(game.Name).SendAsync("ReceiveCurrentPlayer", game.CurrentPlayer);
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "SetTurnPrev: " + ae.Message);
            }

            return -1;
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
