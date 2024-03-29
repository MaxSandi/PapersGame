﻿namespace PapersGame.Backend.Domain
{
    public class Player
    {
        public string Id { get; }
        public string Name { get; }

        /// <summary>
        /// Отгадываемый персонаж
        /// </summary>
        public string Character { get; private set; }
        /// <summary>
        /// Загадываемый персонаж
        /// </summary>
        public string ProposeCharacter { get; set; }
        /// <summary>
        /// Отгадан ли персонаж?
        /// </summary>
        public bool IsGuessed { get; set; }

        public bool IsReady => !string.IsNullOrEmpty(ProposeCharacter);

        public Player(string name)
        {
            Id = Guid.NewGuid().ToString();

            Name = name;
            ProposeCharacter = string.Empty;
            Character = string.Empty;
            IsGuessed = false;
        }

        public void SetCharacter(string character)
        {
            if (!string.IsNullOrEmpty(Character))
                throw new ArgumentException("Player already have character!", Name);

            Character = character;
        }
    }
}
