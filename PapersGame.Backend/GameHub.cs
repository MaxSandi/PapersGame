using Microsoft.AspNetCore.SignalR;
using PapersGame.Backend.Domain;
using PapersGame.Backend.Providers;
using System.Linq;

namespace PapersGame.Backend
{
    public class GameHub : Hub
    {
        private static readonly GameProvider _gameProvider = new GameProvider();

        private static readonly Dictionary<string, (Game game, Player player)> _connectionProvider = new Dictionary<string, (Game game, Player player)>();

        #region Messages
        public async Task SendError(IClientProxy client, string errMessage)
        {
            await client.SendAsync("ReceiveError", errMessage);
        }

        public async Task UserRequestsPlayer()
        {
            if(_connectionProvider.TryGetValue(Context.ConnectionId, out var connectionInfo))
            {
                await Clients.Caller.SendAsync("ReceivePlayer", connectionInfo.player);
            }
        }

        public async Task GroupRequestsPlayerList()
        {
            if (_connectionProvider.TryGetValue(Context.ConnectionId, out var connectionInfo))
            {
                var gameGroup = GetGameGroup(connectionInfo.game.Id);
                if(gameGroup is not null)
                    await gameGroup.SendAsync("ReceivePlayersList", connectionInfo.game.Players);
            }
        }

        public async Task GroupRequestsGameStarted()
        {
            if (_connectionProvider.TryGetValue(Context.ConnectionId, out var connectionInfo))
            {
                var gameGroup = GetGameGroup(connectionInfo.game.Id);
                if (gameGroup is not null)
                    await gameGroup.SendAsync("IGameStarted", connectionInfo.game);
            }
        }

        public async Task GroupRequestsGameStopped(string gameId)
        {
            if (_connectionProvider.TryGetValue(Context.ConnectionId, out var connectionInfo))
            {
                await Clients.Group(gameId).SendAsync("IGameStopped");
            }
        }

        public async Task GroupRequestsCurrentPlayerIndex()
        {
            if (_connectionProvider.TryGetValue(Context.ConnectionId, out var connectionInfo))
            {
                var game = connectionInfo.game;
                var playerIndex = game.Players.FindIndex(x => x.Equals(game.CurrentPlayer));

                var gameGroup = GetGameGroup(connectionInfo.game.Id);
                if (gameGroup is not null)
                    await gameGroup.SendAsync("ReceiveCurrentPlayer", playerIndex);
            }
        }

        #endregion

        public GameHub()
        {

        }
        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await RemoveConnectionInfoAsync();

            await base.OnDisconnectedAsync(exception);
        }

        public async Task CreateGame(string gameName, int playerCount)
        {
            try
            {
                _gameProvider.CreateGame(gameName, playerCount);
                
                await Clients.Caller.SendAsync("IGameCreated", _gameProvider.Game);
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "CreateGame: " + ae.Message);
                throw;
            }
        }

        public async Task JoinGame(string gameId, string userName)
        {
            try
            {
                //TODO: временно, пока не реализован выбор игры
                gameId = string.IsNullOrEmpty(gameId) ? _gameProvider.Game?.Id : gameId;

                var game = GetGame(gameId);
                var player = _gameProvider.JoinGame(gameId, userName);

                await AddConnectionInfoAsync(game, player);
                await Clients.Caller.SendAsync("IGameJoined", game, player);

                await GroupRequestsPlayerList();
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "JoinGame: " + ae.Message);
                throw;
            }
        }

        public async Task ReconnectGame(string gameId, string playerId)
        {
            try
            {
                var game = GetGame(gameId);

                var player = game.GetPlayer(playerId);
                if (player is null)
                    throw new Exception($"Player {playerId} not found in game {gameId}");

                await AddConnectionInfoAsync(game, player);
                await Clients.Caller.SendAsync("IGameReconnected", game);

                await GroupRequestsPlayerList();
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "ReconnectGame: " + ae.Message);
                throw;
            }
        }

        public async Task<bool> CanReconnectGame(string gameId, string playerId)
        {
            try
            {
                var game = GetGame(gameId);
                var player = game?.GetPlayer(playerId);

                return game is not null && player is not null;
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "CanReconnectGame: " + ae.Message);
                throw;
            }
        }

        public async Task SetPlayerReady(string characterName)
        {
            try
            {
                var game = GetGameByConnectionId();
                if (game is null)
                    throw new Exception($"$Can't found game by connection!");
                var player = GetPlayerByConnectionId();
                if (player is null)
                    throw new Exception($"$Can't found player by connection!");

                _gameProvider.SetPlayerReady(game.Id, player.Id, characterName);
                await GroupRequestsPlayerList();
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
                var game = GetGameByConnectionId();
                if (game is null)
                    throw new Exception($"$Can't found game by connection!");
                var player = GetPlayerByConnectionId();
                if (player is null)
                    throw new Exception($"$Can't found player by connection!");

                _gameProvider.SetPlayerUnready(game.Id, player.Id);
                await GroupRequestsPlayerList();
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "SetPlayerUnready: " + ae.Message);
            }
        }

        public async Task StartGame(string gameId)
        {
            try
            {
                var player = GetPlayerByConnectionId();
                if (player is null)
                    throw new Exception($"$Can't found player by connection!");

                _gameProvider.StartGame(gameId, player.Id);
                await GroupRequestsGameStarted();
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "StartGame: " + ae.Message);
            }
        }

        public async Task StopGame(string gameId)
        {
            try
            {
                var player = GetPlayerByConnectionId();
                if (player is null)
                    throw new Exception($"$Can't found player by connection!");

                _gameProvider.StopGame(gameId, player.Id);
                await GroupRequestsGameStopped(gameId);
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "StopGame: " + ae.Message);
            }
        }

        public async Task<bool> CheckPlayerIsAdmin()
        {
            try
            {
                var game = GetGameByConnectionId();
                if (game is null)
                    throw new Exception($"$Can't found game by connection!");
                var player = GetPlayerByConnectionId();
                if (player is null)
                    throw new Exception($"$Can't found player by connection!");

                return player.Id == game.AdminId;
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
                var game = GetGameByConnectionId();
                if (game is null)
                    throw new Exception();

                game.SetTurnNext();
                await GroupRequestsCurrentPlayerIndex();
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "SetTurnNext: " + ae.Message);
            }
        }

        public async Task<int> SetTurnPrevious()
        {
            try
            {
                var game = GetGameByConnectionId();
                if (game is null)
                    throw new Exception();

                game.SetTurnPrevious();
                await GroupRequestsCurrentPlayerIndex();
            }
            catch (Exception ae)
            {
                var client = Clients.Caller;
                await SendError(client, "SetTurnPrevious: " + ae.Message);
            }

            return -1;
        }

        #region Connection provider
        private async Task AddConnectionInfoAsync(Game game, Player player)
        {
            if (_connectionProvider.ContainsKey(Context.ConnectionId))
                await RemoveConnectionInfoAsync();

            _connectionProvider.Add(Context.ConnectionId, new(game, player));
            await Groups.AddToGroupAsync(Context.ConnectionId, game.Id);
        }

        private async Task RemoveConnectionInfoAsync()
        {
            if (_connectionProvider.TryGetValue(Context.ConnectionId, out var connectionInfo))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, connectionInfo.game.Id);

                _connectionProvider.Remove(Context.ConnectionId);
            }
        }
        #endregion

        #region Private methods
        private Game? GetGame(string gameId) 
        {
            if (_gameProvider.Game is null || _gameProvider.Game.Id != gameId)
                return null;

            return _gameProvider.Game;
        }

        private IClientProxy? GetGameGroup(string gameId)
        {
            var game = GetGame(gameId);
            if (game is null)
                return null;

            return Clients.Group(game.Id);
        }

        private Game? GetGameByConnectionId(string? connectionId = null)
        {
            connectionId = string.IsNullOrEmpty(connectionId) ? Context.ConnectionId : connectionId;
            if (!_connectionProvider.TryGetValue(connectionId, out var connectionInfo))
                return null;

            return connectionInfo.game;
        }

        private Player? GetPlayerByConnectionId(string? connectionId = null)
        {
            connectionId = string.IsNullOrEmpty(connectionId) ? Context.ConnectionId : connectionId;
            if (!_connectionProvider.TryGetValue(connectionId, out var connectionInfo))
                return null;

            return connectionInfo.player;
        }
        #endregion
    }
}
