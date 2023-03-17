using Microsoft.AspNetCore.SignalR;
using PapersGame.Backend.Domain;
using PapersGame.Backend.Providers;
using System.Linq;

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

        public async Task JoinGame(string userName, string message)
        {
            var gameId = message.Split(',')[0];
            var isReconnect = message.Split(',')[1].Trim() == "true";
            try
            {
                if (string.IsNullOrEmpty(gameId))
                {
                    gameId = _gameProvider.Game?.Id;
                }

                _gameProvider.JoinGame(userName, gameId, isReconnect);

                var game = GetGameByConnectionId(gameId);
                if (game is not null)
                {
                    var gameName = game.Name;
                    await Groups.AddToGroupAsync(gameId, gameName);
                    await Clients.Caller.SendAsync("IGameJoined", _gameProvider.Game);
                    //TODO update to Clients.Group(...) to work with reconnect
                    await Clients.All.SendAsync("ReceivePlayersList", game.Players);
                }
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "JoinGame: " + ae.Message);
                throw;
            }
        }

        public async Task SetPlayerReady(string characterName, string connectionId)
        {
            try
            {
                _gameProvider.SetPlayerReady(connectionId, characterName);

                if(_gameProvider.Game is not null)
                {
                    //TODO update to Clients.Group(...) to work with reconnect
                    await Clients.All.SendAsync("ReceivePlayersList", _gameProvider.Game.Players);
                }
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "SetPlayerReady: " + ae.Message);
                throw;
            }
        }

        public async Task SetPlayerUnready(string connectionId)
        {
            try
            {
                _gameProvider.SetPlayerUnready(connectionId);

                if (_gameProvider.Game is not null)
                {
                    //TODO update to Clients.Group(...) to work with reconnect
                    await Clients.All.SendAsync("ReceivePlayersList", _gameProvider.Game.Players);
                }
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "SetPlayerUnready: " + ae.Message);
            }
        }

        public async Task StartGame(string connectionId)
        {
            try
            {
                _gameProvider.StartGame(connectionId);

                var game = GetGameByConnectionId(connectionId);
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

        public async Task StopGame(string connectionId)
        {
            try
            {
                var game = GetGameByConnectionId(connectionId);
                if (game is not null)
                {
                    var group = Clients.Group(game.Name);
                    _gameProvider.StopGame(connectionId);

                    await group.SendAsync("IGameStopped");
                }
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "StartGame: " + ae.Message);
            }
        }

        public async Task<bool> CheckPlayerIsAdmin(string connectionId)
        {
            try
            {
                connectionId = connectionId ?? Context.ConnectionId;
                var game = GetGameByConnectionId(connectionId);

                return connectionId == game?.AdminConnectionId;
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
            return _gameProvider.Game.Id == connectionId ? _gameProvider.Game : null;
        }
        #endregion
    }
}
