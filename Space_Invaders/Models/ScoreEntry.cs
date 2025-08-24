using System;

namespace Space_Invaders.Models
{
    public class ScoreEntry
    {
        public DateTime Date { get; set; }
        public string PlayerName { get; set; }
        public int Score { get; set; }

        public override string ToString()
        {
            return $"{Date.ToShortDateString()} {Date.ToShortTimeString()}: {PlayerName} - {Score} points";
        }
    }
}