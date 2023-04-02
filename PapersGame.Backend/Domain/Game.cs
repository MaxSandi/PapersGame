using System.Collections.Concurrent;

namespace PapersGame.Backend.Domain
{
    public class Game
    {
        public string Id { get; }

        public string Name { get; }
        public List<Player> Players { get; }
        public Player CurrentPlayer { get; set; }
        /// <summary>
        /// Персонажи, загаданные игроками
        /// </summary>
        public int PlayersLimit { get; private set; }
        public bool IsStarted { get; private set; }

        public string AdminId { get; private set; }

        public bool IsReady => Players.Count > 1 && Players.All(p => p.IsReady);

        public Game(string name, int playerLimit)
        {
            if(playerLimit <= 1)
                throw new ArgumentException("Player count must be greater then 1!");

            Id = Guid.NewGuid().ToString();
            Name = name;
            PlayersLimit = playerLimit;
            Players = new List<Player>();
            IsStarted = false;
        }

        /// <summary>
        /// Добавить игрока к игре
        /// </summary>
        /// <param name="userName">Имя игрока</param>
        /// <returns>Добавленный игрок</returns>
        internal Player AddPlayer(string userName)
        {
            if (PlayersLimit == 0 && !Players.Exists(x => x.Name == userName))
                throw new Exception("Game room is full");

            //TODO: временная условность для избежания непоняток с одинаковыми именами
            if (Players.Exists(x => x.Name == userName))
                throw new ArgumentException("Player with this name already exists", userName);

            // первый добавленный игрок является админом
            var player = new Player(userName);
            if (!Players.Any())
                AdminId = player.Id;

            Players.Add(player);
            PlayersLimit--;
            
            return player;
        }

        /// <summary>
        /// Запустить игру
        /// </summary>
        internal void Start()
        {
            //TODO: shuffle players

            SetCharacterToPlayers();

            CurrentPlayer = Players.First();

            IsStarted = true;
        }

        /// <summary>
        /// Остановить игру
        /// </summary>
        internal void Stop()
        {
            IsStarted = false;
        }

        /// <summary>
        /// Передать ход следующему игроку
        /// </summary>
        /// <returns>Номер текущего игрока</returns>
        internal int SetTurnNext()
        {
            var currentPlayerIndex = Players.FindIndex(x => x.Equals(CurrentPlayer));
            if (currentPlayerIndex == -1)
                throw new Exception("Player not exist!");

            currentPlayerIndex++;
            if (currentPlayerIndex >= Players.Count)
                currentPlayerIndex = 0;

            CurrentPlayer = Players[currentPlayerIndex];
            return currentPlayerIndex;
        }

        /// <summary>
        /// Передать ход предыдущему игроку
        /// </summary>
        /// <returns>Номер текущего игрока</returns>
        internal int SetTurnPrevious()
        {
            var currentPlayerIndex = Players.FindIndex(x => x.Equals(CurrentPlayer));
            if (currentPlayerIndex == -1)
                throw new Exception("Player not exist!");

            currentPlayerIndex--;
            if (currentPlayerIndex < 0)
                currentPlayerIndex = Players.Count - 1;

            CurrentPlayer = Players[currentPlayerIndex];
            return currentPlayerIndex;
        }

        internal Player GetPlayer(string playerId)
        {
            return Players.Single(x => x.Id == playerId);
        }

        internal bool IsPlayerReady(string playerId)
        {
            return Players.Single(x => x.Id == playerId).IsReady;
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
