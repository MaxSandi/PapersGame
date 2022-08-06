namespace PapersGame.Backend.Domain
{
    public class Player
    {
        public string Name { get; }
        public string ConnectionId { get; }

        public bool IsReady => !string.IsNullOrEmpty(ProposeCharacter);

        /// <summary>
        /// Загадываемый персонаж
        /// </summary>
        public string ProposeCharacter { get; set; }

        /// <summary>
        /// Отгадываемый персонаж
        /// </summary>
        public string GuessedCharacter { get; private set; }
        /// <summary>
        /// Отгадан ли персонаж?
        /// </summary>
        public bool IsGuessed { get; set; }

        public Player(string name, string connectionId)
        {
            Name = name;
            ConnectionId = connectionId;
            ProposeCharacter = string.Empty;
            GuessedCharacter = string.Empty;
            IsGuessed = false;
        }

        public void SetGuessedCharacter(string character)
        {
            if (!string.IsNullOrEmpty(GuessedCharacter))
                throw new ArgumentException("Player already have character!", Name);

            GuessedCharacter = character;
        }
    }
}
