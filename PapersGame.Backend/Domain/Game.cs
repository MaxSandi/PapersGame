using System.Collections.Concurrent;

namespace PapersGame.Backend.Domain
{
    public class Game
    {
        public string Id { get; } = "111";

        public string Name { get; }
        public List<Player> Players { get; }
        /// <summary>
        /// Персонажи, загаданные игроками
        /// </summary>
        public int PlayersLimit { get; private set; }
        public bool Started { get; private set; }

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
            Started = false;
        }

        /// <summary>
        /// Добавить игрока к игре
        /// </summary>
        /// <param name="userName">Имя игрока</param>
        /// <param name="connectionId">Идентификатор подключения</param>
        /// <returns>Добавленный игрок</returns>
        internal Player AddPlayer(string userName, string connectionId)
        {
            if (PlayersLimit == 0)
                throw new Exception("Game room is full");

            //TODO: временная условность для избежания непоняток с одинаковыми именами
            if (Players.Exists(x => x.Name == userName))
                throw new ArgumentException("Player with this name already exists", userName);

            var player = new Player(userName, connectionId);
            Players.Add(player);
            PlayersLimit--;
            return player;
        }

        internal void Start()
        {
            SetCharacterToPlayers();

            Started = true;
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
            var characters = Players.Select(player => (player, player.ProposeCharacter)).ToDictionary(x => x.player, x => x.ProposeCharacter);
            foreach (var player in Players)
            {
                var item = characters.OrderBy(x => Guid.NewGuid()).Where(x => x.Key != player).First();
                player.SetGuessedCharacter(item.Value);

                characters.Remove(item.Key);
            }
        }
        #endregion
    }
}
