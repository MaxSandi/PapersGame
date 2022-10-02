using PapersGame.Backend.Domain;

namespace PapersGame.Backend.Providers
{
    public class GameProvider
    {
        public Game? Game { get; private set; }

        public void CreateGame(string gameName, int playerCount, string connectionId)
        {
            if (Game is not null)
                throw new Exception("Game already exist!");

            Game = new Game(gameName, playerCount, connectionId);
        }

        public void JoinToGame(string playerName, string connectionId)
        {
            if (Game is null)
                throw new Exception("Game hasn't been created yet!");

            if (Game.IsStarted)
                throw new Exception("Can't add player. Game was started!");

            Game.AddPlayer(playerName, connectionId);
        }

        public void SetPlayerReady(string connectionId, string characterName)
        {
            if (Game is null)
                throw new Exception("Game hasn't been created yet!");

            if (string.IsNullOrEmpty(characterName))
                throw new Exception("Character name can't by empty!");

            var player = Game.GetPlayer(connectionId);
            player.ProposeCharacter = characterName;
        }

        public void SetPlayerUnready(string connectionId)
        {
            if (Game is null)
                throw new Exception("Game hasn't been created yet!");

            var player = Game.GetPlayer(connectionId);
            player.ProposeCharacter = string.Empty;
        }

        internal void StartGame(string connectionId)
        {
            if (Game is null)
                throw new Exception("Game hasn't been created yet!");

            if(!Game.IsReady)
                throw new Exception("Game is not ready yet!");

            var player = Game.GetPlayer(connectionId);
            if(player.ConnectionId != Game.AdminConnectionId)
                throw new Exception("Only admin can start game!");

            Game.Start();
        }

        internal void StopGame(string connectionId)
        {
            if (Game is null)
                throw new Exception("Game hasn't been created yet!");

            var player = Game.GetPlayer(connectionId);
            if (player.ConnectionId != Game.AdminConnectionId)
                throw new Exception("Only admin can stop game!");

            Game.Stop();
            Game = null;//TODO: remove game from list
        }
    }
}
