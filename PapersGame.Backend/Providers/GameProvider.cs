using PapersGame.Backend.Domain;

namespace PapersGame.Backend.Providers
{
    public class GameProvider
    {
        public Game? Game { get; private set; }

        public void CreateGame(string gameName, int playerCount)
        {
            if (Game is not null)
                throw new Exception("Game already exist!");

            Game = new Game(gameName, playerCount);
        }

        public Player JoinGame(string gameId, string playerName)
        {
            // TODO: use game id to find a game

            if (Game is null)
                throw new Exception("Game hasn't been created yet!");

            if (Game.IsStarted)
              throw new Exception("Can't add player. Game was started!");

            return Game.AddPlayer(playerName);
        }

        public void SetPlayerReady(string gameId, string playerId, string characterName)
        {
            // TODO: use game id to find a game

            if (Game is null)
                throw new Exception("Game hasn't been created yet!");

            if (string.IsNullOrEmpty(characterName))
                throw new Exception("Character name can't by empty!");

            var player = Game.GetPlayer(playerId);
            player.ProposeCharacter = characterName;
        }

        public void SetPlayerUnready(string gameId, string playerId)
        {
            // TODO: use game id to find a game

            if (Game is null)
                throw new Exception("Game hasn't been created yet!");

            var player = Game.GetPlayer(playerId);
            player.ProposeCharacter = string.Empty;
        }

        internal void StartGame(string gameId, string playerId)
        {
            // TODO: use game id to find a game

            if (Game is null)
                throw new Exception("Game hasn't been created yet!");

            if(!Game.IsReady)
                throw new Exception("Game is not ready yet!");

            if(playerId != Game.AdminId)
                throw new Exception("Only admin can start game!");

            Game.Start();
        }

        internal void StopGame(string gameId, string playerId)
        {
            // TODO: use game id to find a game

            if (Game is null)
                throw new Exception("Game hasn't been created yet!");

            if (playerId != Game.AdminId)
                throw new Exception("Only admin can start game!");

            Game.Stop();
            Game = null;//TODO: remove game from list
        }
    }
}
