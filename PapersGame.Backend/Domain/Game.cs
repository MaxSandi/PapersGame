using System.Collections.Concurrent;

namespace PapersGame.Backend.Domain
{
    public class Game
    {
        public string Id { get; } = "111";

        public string Name { get; }
        public List<Player> Players { get; }
        public Player CurrentPlayer { get; set; }
        /// <summary>
        /// Персонажи, загаданные игроками
        /// </summary>
        public int PlayersLimit { get; private set; }
        public bool IsStarted { get; private set; }

        public string AdminConnectionId { get; private set; }

        public bool IsReady => Players.Count > 1 && Players.All(p => p.IsReady);

        public Game(string name, int playerLimit, string adminConnectionId)
        {
            if(playerLimit <= 1)
                throw new ArgumentException("Player count must be greater then 1!");

            Name = name;
            PlayersLimit = playerLimit;
            AdminConnectionId = adminConnectionId;
            Players = new List<Player>();
            IsStarted = false;
        }

        /// <summary>
        /// Добавить игрока к игре
        /// </summary>
        /// <param name="userName">Имя игрока</param>
        /// <param name="connectionId">Идентификатор подключения</param>
        /// <returns>Добавленный игрок</returns>
        internal Player AddPlayer(string userName, string connectionId, bool isReconnect)
        {
            if (PlayersLimit == 0)
                throw new Exception("Game room is full");

            //TODO: временная условность для избежания непоняток с одинаковыми именами
            if (!isReconnect && Players.Exists(x => x.Name == userName))
                throw new ArgumentException("Player with this name already exists", userName);

            var player = new Player(userName, connectionId);
            if(!isReconnect)
            {
                Players.Add(player);
                PlayersLimit--;
            }
            
            return player;
        }

        internal void Start()
        {
            //TODO: shuffle players

            SetCharacterToPlayers();

            CurrentPlayer = Players.First();

            IsStarted = true;
        }

        internal void Stop()
        {
            IsStarted = false;
        }

        internal Player GetPlayer(string connectionId)
        {
            return Players.First(x => x.ConnectionId == connectionId);
        }

        internal bool IsPlayerReady(string connectionId)
        {
            return Players.First(x => x.ConnectionId == connectionId).IsReady;
        }

        #region Private methods
        private void SetCharacterToPlayers()
        {
            Random rnd = new Random();

            var characters = Players.Select(player => (player, player.ProposeCharacter))
                                    .ToDictionary(x => x.player, x => x.ProposeCharacter);

            // For an odd number of Players, there is a case when the last Player doesn't have the appropriate character.
            if (Players.Count() % 2 != 0)
            {
                int randomPosition  = rnd.Next(Players.Count() - 1);
                var randomElement = characters.ElementAt(randomPosition);
                Players.Last().SetCharacter(randomElement.Value);
                characters.Remove(randomElement.Key);
            }
            
            foreach (var player in Players)
            {
                if (!string.IsNullOrEmpty(player.Character))
                {
                    continue;
                }
                
                var activeCharacters = characters.Where(x => x.Key != player);

                int randomPosition = rnd.Next(activeCharacters.Count());
                var randomElement = activeCharacters.ElementAt(randomPosition);

                player.SetCharacter(randomElement.Value);
                characters.Remove(randomElement.Key);
            }
        }
        #endregion
    }
}
